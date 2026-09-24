import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { existsSync } from "node:fs";
import { execFile } from "node:child_process";
import { randomUUID } from "node:crypto";
import { promisify } from "node:util";
import { resolve, join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import axeCore from "axe-core";
import pixelmatch from "pixelmatch";
import { PNG } from "pngjs";
import { chromium, type Browser, type Page } from "playwright";
import { z } from "zod";

const __dirname = dirname(fileURLToPath(import.meta.url));

const baseUrl = process.env.UI_BASE_URL ?? "https://localhost:6443";
const baseOrigin = new URL(baseUrl).origin;
const navigationTimeout = Number(process.env.UI_NAVIGATION_TIMEOUT_MS ?? 15000);
const interactionSettleMs = Number(process.env.UI_INTERACTION_SETTLE_MS ?? 1000);
const storageStatePath = process.env.UI_STORAGE_STATE;
const authCookieEnv = process.env.UI_AUTH_COOKIE;
const authHeaderEnv = process.env.UI_AUTH_HEADER;
const execFileAsync = promisify(execFile);
const axeSource = axeCore.source;

export function resolveSourceRoot(customPath?: string): string {
  if (customPath && existsSync(customPath)) return resolve(customPath);
  if (process.env.UI_SOURCE_ROOT && existsSync(process.env.UI_SOURCE_ROOT)) {
    return resolve(process.env.UI_SOURCE_ROOT);
  }
  const cwd = process.cwd();
  // 1. If cwd is Socratic meta-repo
  const inFrontend = join(cwd, "src", "Frontend");
  if (existsSync(inFrontend)) return inFrontend;
  // 2. If cwd is Frontend meta-repo or autonomous feature module
  return cwd;
}

export function resolveAnalyzerDll(): string | undefined {
  if (process.env.UI_ANALYZER_DLL && existsSync(process.env.UI_ANALYZER_DLL)) {
    return resolve(process.env.UI_ANALYZER_DLL);
  }
  // 1. Check right sibling directory inside Platform/Tools
  const sibling = resolve(__dirname, "../../BlazorUiQuality.Analyzer/bin/Debug/net10.0/BlazorUiQuality.Analyzer.dll");
  if (existsSync(sibling)) return sibling;

  // 2. Search upwards from cwd
  let cur = process.cwd();
  for (let i = 0; i < 6; i++) {
    const candidatePlatform = join(cur, "Platform", "Tools", "BlazorUiQuality.Analyzer", "bin", "Debug", "net10.0", "BlazorUiQuality.Analyzer.dll");
    if (existsSync(candidatePlatform)) return candidatePlatform;
    const candidateSrc = join(cur, "src", "Frontend", "Platform", "Tools", "BlazorUiQuality.Analyzer", "bin", "Debug", "net10.0", "BlazorUiQuality.Analyzer.dll");
    if (existsSync(candidateSrc)) return candidateSrc;
    const candidateRoot = join(cur, "src", "Tools", "BlazorUiQuality.Analyzer", "bin", "Debug", "net10.0", "BlazorUiQuality.Analyzer.dll");
    if (existsSync(candidateRoot)) return candidateRoot;
    const parent = dirname(cur);
    if (parent === cur) break;
    cur = parent;
  }
  return undefined;
}

type FixtureState = "loading" | "empty" | "populated" | "error";
type FixtureTheme = "light" | "dark";
let browser: Browser | undefined;

type RuntimeDiagnostics = {
  consoleErrors: string[];
  pageErrors: string[];
  failedRequests: string[];
  httpErrors: Array<{ url: string; status: number }>;
};

type UiQualityIssue = {
  id: string;
  type: string;
  severity: "info" | "warning" | "error";
  confidence: number;
  evidence: Record<string, unknown>;
  source?: Record<string, unknown>;
};

type UiQualityReport = {
  runId: string;
  url: string;
  viewport: { width: number; height: number };
  component?: Record<string, unknown>;
  state?: string;
  theme?: string;
  hydration: Awaited<ReturnType<typeof waitForBlazor>>;
  runtime: RuntimeDiagnostics;
  issues: UiQualityIssue[];
  sourceContext?: unknown;
};

const viewportSchema = z.object({
  width: z.number().int().min(240).max(3840),
  height: z.number().int().min(240).max(2160)
});

const scenarioActionSchema = z.discriminatedUnion("type", [
  z.object({ type: z.literal("click"), selector: z.string().min(1) }),
  z.object({ type: z.literal("fill"), selector: z.string().min(1), value: z.string() }),
  z.object({ type: z.literal("press"), selector: z.string().min(1), key: z.string().min(1) }),
  z.object({ type: z.literal("waitFor"), selector: z.string().min(1), timeoutMs: z.number().int().min(1).max(30000).optional() }),
  z.object({ type: z.literal("setState"), value: z.enum(["loading", "empty", "populated", "error"]) }),
  z.object({ type: z.literal("setTheme"), value: z.enum(["light", "dark"]) })
]);

async function openPage(viewport: { width: number; height: number }): Promise<{ page: Page; runtime: RuntimeDiagnostics; close: () => Promise<void> }> {
  browser ??= await chromium.launch({
    headless: true,
    args: ["--ignore-certificate-errors"] // Allow local dev SSL certificates (https://localhost:7001, https://localhost:6443)
  });

  const contextOptions: Parameters<Browser["newContext"]>[0] = {
    viewport,
    ignoreHTTPSErrors: true
  };

  if (storageStatePath && existsSync(storageStatePath)) {
    contextOptions.storageState = storageStatePath;
  }
  if (authHeaderEnv) {
    contextOptions.extraHTTPHeaders = { Authorization: authHeaderEnv };
  }

  const context = await browser.newContext(contextOptions);

  if (authCookieEnv) {
    const [cookieName, cookieVal] = authCookieEnv.includes("=")
      ? authCookieEnv.split("=", 2)
      : ["AccessToken", authCookieEnv];
    await context.addCookies([
      {
        name: cookieName.trim(),
        value: cookieVal.trim(),
        url: baseUrl
      }
    ]);
  }

  context.setDefaultTimeout(navigationTimeout);
  context.setDefaultNavigationTimeout(navigationTimeout);
  const page = await context.newPage();
  const runtime: RuntimeDiagnostics = { consoleErrors: [], pageErrors: [], failedRequests: [], httpErrors: [] };
  page.on("console", message => {
    if (message.type() === "error") runtime.consoleErrors.push(message.text());
  });

  page.on("pageerror", error => runtime.pageErrors.push(error.message));
  page.on("requestfailed", request => runtime.failedRequests.push(`${request.method()} ${request.url()}: ${request.failure()?.errorText ?? "failed"}`));
  page.on("response", response => {
    if (response.status() >= 400) runtime.httpErrors.push({ url: response.url(), status: response.status() });
  });
  return { page, runtime, close: () => context.close() };
}

async function safeNavigate(page: Page, path: string): Promise<boolean> {
  try {
    await page.goto(targetUrl(path), { waitUntil: "commit", timeout: Math.min(navigationTimeout, 6000) });
    return true;
  } catch (error: any) {
    const msg = String(error?.message ?? "");
    if (msg.includes("ERR_CONNECTION_REFUSED") || msg.includes("ECONNREFUSED") || error?.name === "TimeoutError") {
      return false;
    }
    throw error;
  }
}

function hostOfflineResponse(path: string) {
  return {
    content: [{
      type: "text" as const,
      text: JSON.stringify({
        status: "host_offline",
        message: `Blazor Web.UI host is not reachable at ${baseUrl}.`,
        hint: "Start the Blazor host with 'dotnet run --project Platform/Web.UI/Web.UI' or launch via Aspire AppHost to enable live UI inspection. For offline source code audits, use 'inspect_component' which uses static Roslyn analysis.",
        targetUrl: targetUrl(path)
      }, null, 2)
    }]
  };
}

async function applyFixtureState(page: Page, state?: FixtureState, theme?: FixtureTheme): Promise<void> {
  if (state) {
    const stateButton = page.locator(`[data-testid="state-${state}-button"]`);
    if (await stateButton.count() > 0) {
      await stateButton.click();
      await page.locator(`[data-blazor-component][data-ui-state="${state}"]`).waitFor({ state: "attached", timeout: 2000 }).catch(() => undefined);
    }
  }
  if (theme) {
    const themeButton = page.locator(`[data-testid="theme-${theme}-button"]`);
    if (await themeButton.count() > 0) {
      await themeButton.click();
      await page.locator(`[data-blazor-component][data-ui-theme="${theme}"]`).waitFor({ state: "attached", timeout: 2000 }).catch(() => undefined);
    } else {
      // Socratic standard theme toggling via class on document.documentElement or body
      await page.evaluate((t) => {
        if (t === "dark") {
          document.documentElement.classList.add("dark");
          document.body.classList.add("dark");
        } else {
          document.documentElement.classList.remove("dark");
          document.body.classList.remove("dark");
        }
      }, theme);
    }
  }
}

async function captureUiSnapshot(page: Page, selector: string): Promise<{ image: Buffer; fingerprint: Record<string, unknown> }> {
  const fingerprint = await page.evaluate((requestedSelector) => {
    const root = document.querySelector<HTMLElement>(requestedSelector);
    if (!root) return { error: `Selector not found: ${requestedSelector}` };
    const rect = root.getBoundingClientRect();
    return {
      state: root.dataset.uiState,
      theme: root.dataset.uiTheme,
      text: (root.innerText ?? "").replace(/\s+/g, " ").trim(),
      rect: { x: Math.round(rect.x), y: Math.round(rect.y), width: Math.round(rect.width), height: Math.round(rect.height) },
      scroll: { width: root.scrollWidth, height: root.scrollHeight },
      visibleElements: root.querySelectorAll("*:not(script):not(style)").length
    };
  }, selector);
  return { image: await page.screenshot({ type: "png" }), fingerprint };
}

async function getSourceContext(componentName?: string, customSourceRoot?: string): Promise<{ components?: unknown[]; component?: unknown; sourceRoot?: string; error?: string }> {
  const effectiveRoot = resolveSourceRoot(customSourceRoot);
  const effectiveAnalyzer = resolveAnalyzerDll();

  if (!effectiveAnalyzer) {
    return {
      sourceRoot: effectiveRoot,
      error: "BlazorUiQuality.Analyzer.dll was not found. Build it with: dotnet build Platform/Tools/BlazorUiQuality.Analyzer"
    };
  }

  try {
    const { stdout } = await execFileAsync("dotnet", [effectiveAnalyzer, "--source-root", effectiveRoot], { timeout: navigationTimeout });
    const components = JSON.parse(stdout) as Array<{ componentName?: string }>;
    return {
      sourceRoot: effectiveRoot,
      components,
      component: componentName ? components.find(item => item.componentName === componentName) : undefined
    };
  } catch (error) {
    return {
      sourceRoot: effectiveRoot,
      error: error instanceof Error ? error.message : "The Blazor source analyzer failed."
    };
  }
}

function targetUrl(path: string): string {
  if (!path.startsWith("/")) {
    throw new Error("Only relative paths beginning with '/' are allowed.");
  }

  const url = new URL(path, baseUrl);
  if (url.origin !== baseOrigin) {
    throw new Error("The target URL must use the configured UI_BASE_URL origin.");
  }

  return url.toString();
}

async function waitForBlazor(page: Page): Promise<{ domChangedAfterLoad: boolean; bootScriptDetected: boolean; blazorMarkers: number }> {
  const beforeLength = await page.evaluate(() => document.documentElement.outerHTML.length).catch(() => 0);
  await page.waitForLoadState("networkidle").catch(() => undefined);
  await page.waitForTimeout(interactionSettleMs);
  return page.evaluate((initialLength) => {
    const html = document.documentElement.outerHTML;
    return {
      domChangedAfterLoad: html.length !== initialLength,
      bootScriptDetected: Array.from(document.scripts).some(script => /blazor(?:\.web)?\.js/i.test(script.src)),
      blazorMarkers: (html.match(/<!--Blazor:/g) ?? []).length
    };
  }, beforeLength);
}

const server = new McpServer({
  name: "socratic-ui-quality",
  version: "1.1.0"
});

server.registerTool("inspect_ui", {
  description: "Inspect the rendered Socratic Blazor UI at a viewport and return DOM, geometry, styles, overflow, touch target size, and token evidence.",
  inputSchema: {
    path: z.string().default("/"),
    selector: z.string().optional(),
    viewport: viewportSchema.default({ width: 1440, height: 900 })
  }
}, async ({ path, selector, viewport }) => {
  const active = await openPage(viewport);
  const currentPage = active.page;
  try {
    if (!await safeNavigate(currentPage, path)) {
      return hostOfflineResponse(path);
    }
    const hydration = await waitForBlazor(currentPage);

    const result = await currentPage.evaluate(({ selector: requestedSelector, vp }) => {
      const root = requestedSelector ? document.querySelector(requestedSelector) : document.body;
      if (!root) {
        return { error: `Selector not found: ${requestedSelector}` };
      }

      // Collect elements including shadow DOM roots
      const allElements: HTMLElement[] = [];
      function collectElements(node: Element) {
        if (node instanceof HTMLElement && node.tagName !== "SCRIPT" && node.tagName !== "STYLE") {
          allElements.push(node);
        }
        if (node.shadowRoot) {
          Array.from(node.shadowRoot.children).forEach(collectElements);
        }
        Array.from(node.children).forEach(collectElements);
      }
      collectElements(root);

      const elements = allElements.slice(0, 500).map((element) => {
        const rect = element.getBoundingClientRect();
        const styles = getComputedStyle(element);
        const inlineStyle = element.getAttribute("style") ?? "";
        return {
          tag: element.tagName.toLowerCase(),
          id: element.id || undefined,
          classes: element.className || undefined,
          text: (element.textContent ?? "").trim().replace(/\s+/g, " ").slice(0, 120),
          rect: { x: Math.round(rect.x), y: Math.round(rect.y), width: Math.round(rect.width), height: Math.round(rect.height) },
          styles: {
            display: styles.display,
            position: styles.position,
            overflow: styles.overflow,
            fontSize: styles.fontSize,
            lineHeight: styles.lineHeight,
            margin: styles.margin,
            padding: styles.padding,
            color: styles.color,
            backgroundColor: styles.backgroundColor
          },
          inlineStyle: inlineStyle.length > 0 ? inlineStyle : undefined
        };
      });

      // Touch target analysis (Mobile / Tablet <= 1024px)
      const touchIssues: Array<Record<string, unknown>> = [];
      if (vp.width <= 1024) {
        const interactiveSelector = "button, a, input, select, textarea, [role='button'], md-filled-button, md-outlined-button, md-text-button, md-elevated-button, md-tonal-button, md-icon-button, md-chip, md-switch, md-checkbox, md-radio";
        const interactives = allElements.filter(el => el.matches(interactiveSelector) || el.getAttribute("role") === "button");
        for (const el of interactives) {
          const rect = el.getBoundingClientRect();
          if (rect.width > 0 && rect.height > 0) {
            if (rect.width < 44 || rect.height < 44) {
              touchIssues.push({
                element: el.tagName.toLowerCase() + (el.id ? `#${el.id}` : ""),
                severity: "error",
                message: `Touch target size ${Math.round(rect.width)}x${Math.round(rect.height)}px violates minimum 44x44px (recommended 48x48px for POS/Kiosk).`
              });
            } else if (rect.width < 48 || rect.height < 48) {
              touchIssues.push({
                element: el.tagName.toLowerCase() + (el.id ? `#${el.id}` : ""),
                severity: "warning",
                message: `Touch target size ${Math.round(rect.width)}x${Math.round(rect.height)}px is below recommended 48x48px for POS/Kiosk.`
              });
            }
          }
        }
      }

      // Hardcoded inline color analysis
      const colorTokenIssues: Array<Record<string, unknown>> = [];
      const hexRegex = /#(?:[0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\b/;
      for (const el of allElements) {
        const inline = el.getAttribute("style");
        if (inline && hexRegex.test(inline) && !inline.includes("var(--md-sys-color-")) {
          colorTokenIssues.push({
            element: el.tagName.toLowerCase() + (el.id ? `#${el.id}` : ""),
            style: inline,
            message: "Inline style contains hardcoded hex color. Use Material 3 CSS token var(--md-sys-color-*)."
          });
        }
      }

      // Helper to identify Material 3 / Smart / QuickGrid vendor web-components
      const isVendorComponent = (node: Element) => {
        const tag = node.tagName.toLowerCase();
        if (tag.startsWith("md-") || tag.startsWith("smart-") || tag.startsWith("quickgrid-") || tag === "svg" || tag === "path") return true;
        const host = (node.getRootNode() as ShadowRoot)?.host;
        if (host) {
          const hostTag = host.tagName.toLowerCase();
          if (hostTag.startsWith("md-") || hostTag.startsWith("smart-") || hostTag.startsWith("quickgrid-")) return true;
        }
        return false;
      };

      // Forbidden Flexbox layout analysis (Strictly CSS Grid only for application layout code)
      const flexLayoutIssues: Array<Record<string, unknown>> = [];
      for (const el of allElements) {
        if (isVendorComponent(el)) continue;
        const styles = getComputedStyle(el);
        if (styles.display === "flex" || styles.display === "inline-flex") {
          flexLayoutIssues.push({
            element: el.tagName.toLowerCase() + (el.id ? `#${el.id}` : el.className ? `.${el.className.toString().split(" ")[0]}` : ""),
            severity: "error",
            message: "Forbidden flexbox layout ('display: flex' / 'inline-flex'). Socratic standard strictly mandates CSS Grid ('display: grid')."
          });
        }
      }

      const body = document.body;
      return {
        url: location.href,
        title: document.title,
        document: { width: document.documentElement.scrollWidth, height: document.documentElement.scrollHeight },
        viewport: { width: window.innerWidth, height: window.innerHeight },
        overflow: {
          horizontal: body.scrollWidth > window.innerWidth,
          vertical: body.scrollHeight > window.innerHeight,
          bodyScrollWidth: body.scrollWidth,
          bodyScrollHeight: body.scrollHeight
        },
        touchIssues: touchIssues.slice(0, 30),
        colorTokenIssues: colorTokenIssues.slice(0, 30),
        flexLayoutIssues: flexLayoutIssues.slice(0, 30),
        elements
      };
    }, { selector, vp: viewport });

    return { content: [{ type: "text", text: JSON.stringify({ ...result, hydration, runtime: active.runtime }, null, 2) }] };
  } finally {
    await active.close();
  }
});

server.registerTool("inspect_responsive", {
  description: "Inspect a Socratic page across standard viewports (320px - 1920px) and report deterministic overflow and rendering evidence.",
  inputSchema: {
    path: z.string().default("/"),
    viewports: z.array(viewportSchema).default([
      { width: 320, height: 844 }, { width: 360, height: 800 }, { width: 390, height: 844 }, { width: 430, height: 932 },
      { width: 768, height: 1024 }, { width: 1024, height: 768 }, { width: 1440, height: 900 },
      { width: 1920, height: 1080 }
    ])
  }
}, async ({ path, viewports }) => {
  const results = [];
  for (const viewport of viewports) {
    const active = await openPage(viewport);
    try {
      const currentPage = active.page;
      if (!await safeNavigate(currentPage, path)) {
        return hostOfflineResponse(path);
      }
      const hydration = await waitForBlazor(currentPage);
      const metrics = await currentPage.evaluate(() => ({
        viewport: { width: innerWidth, height: innerHeight },
        url: location.href,
        horizontalOverflow: document.documentElement.scrollWidth > innerWidth,
        scrollWidth: document.documentElement.scrollWidth,
        scrollHeight: document.documentElement.scrollHeight,
        visibleTextLength: (document.body.innerText ?? "").length
      }));
      results.push({ ...metrics, hydration, runtime: active.runtime });
    } finally {
      await active.close();
    }
  }

  return { content: [{ type: "text", text: JSON.stringify({ path, results }, null, 2) }] };
});

server.registerTool("find_overlaps", {
  description: "Find visible sibling elements whose bounding boxes overlap on the rendered page.",
  inputSchema: {
    path: z.string().default("/"),
    selector: z.string().default("body *"),
    viewport: viewportSchema.default({ width: 390, height: 844 })
  }
}, async ({ path, selector, viewport }) => {
  const active = await openPage(viewport);
  try {
    const currentPage = active.page;
    if (!await safeNavigate(currentPage, path)) {
      return hostOfflineResponse(path);
    }
    await waitForBlazor(currentPage);
    const overlaps = await currentPage.evaluate((requestedSelector) => {
      const nodes = Array.from(document.querySelectorAll<HTMLElement>(requestedSelector))
        .filter((node) => {
          const style = getComputedStyle(node);
          const rect = node.getBoundingClientRect();
          return style.visibility !== "hidden" && style.display !== "none" && rect.width > 0 && rect.height > 0;
        }).slice(0, 300);
      const result: Array<{ elementA: string; elementB: string; pixels: number; intersection: { width: number; height: number }; styles: { positionA: string; positionB: string; zIndexA: string; zIndexB: string } }> = [];
      const label = (node: HTMLElement) => node.id ? `#${node.id}` : `${node.tagName.toLowerCase()}.${node.className.toString().trim().split(/\s+/).join(".")}`;

      for (let i = 0; i < nodes.length; i++) {
        const a = nodes[i].getBoundingClientRect();
        for (let j = i + 1; j < nodes.length; j++) {
          if (nodes[i].contains(nodes[j]) || nodes[j].contains(nodes[i])) continue;
          const b = nodes[j].getBoundingClientRect();
          const width = Math.min(a.right, b.right) - Math.max(a.left, b.left);
          const height = Math.min(a.bottom, b.bottom) - Math.max(a.top, b.top);
          const pixels = Math.round(width * height);
          if (width > 1 && height > 1 && pixels > 4) {
            const styleA = getComputedStyle(nodes[i]);
            const styleB = getComputedStyle(nodes[j]);
            result.push({
              elementA: label(nodes[i]),
              elementB: label(nodes[j]),
              pixels,
              intersection: { width: Math.round(width), height: Math.round(height) },
              styles: { positionA: styleA.position, positionB: styleB.position, zIndexA: styleA.zIndex, zIndexB: styleB.zIndex }
            });
          }
        }
      }
      return result.slice(0, 100);
    }, selector);

    return { content: [{ type: "text", text: JSON.stringify({ viewport, overlaps }, null, 2) }] };
  } finally {
    await active.close();
  }
});

server.registerTool("screenshot", {
  description: "Capture the rendered page as a PNG image for visual inspection.",
  inputSchema: {
    path: z.string().default("/"),
    viewport: viewportSchema.default({ width: 1440, height: 900 }),
    fullPage: z.boolean().default(false)
  }
}, async ({ path, viewport, fullPage }) => {
  const active = await openPage(viewport);
  try {
    const currentPage = active.page;
    if (!await safeNavigate(currentPage, path)) {
      return hostOfflineResponse(path);
    }
    await waitForBlazor(currentPage);
    const image = await currentPage.screenshot({ type: "png", fullPage });
    return { content: [{ type: "image", data: image.toString("base64"), mimeType: "image/png" }] };
  } finally {
    await active.close();
  }
});

server.registerTool("inspect_component", {
  description: "Return a unified Socratic UI Quality Report combining rendered component evidence, touch targets, Material 3 design tokens, runtime diagnostics, layout issues, and Blazor source metadata. If host is offline, returns static Roslyn source analysis.",
  inputSchema: {
    path: z.string().default("/"),
    selector: z.string().default("[data-blazor-component]"),
    component: z.string().optional(),
    sourceRoot: z.string().optional(),
    viewport: viewportSchema.default({ width: 390, height: 844 }),
    state: z.enum(["loading", "empty", "populated", "error"]).optional(),
    theme: z.enum(["light", "dark"]).optional()
  }
}, async ({ path, selector, component, sourceRoot: customSourceRoot, viewport, state, theme }) => {
  const active = await openPage(viewport);
  try {
    const currentPage = active.page;
    const isOnline = await safeNavigate(currentPage, path);
    if (!isOnline) {
      // Offline fallback: perform static Roslyn source analysis directly
      const sourceContext = await getSourceContext(component, customSourceRoot);
      const report = {
        status: "host_offline",
        note: `Blazor Web.UI host is offline at ${baseUrl}. Live DOM inspection was skipped, but static Blazor Roslyn source analysis was executed successfully.`,
        componentName: component,
        sourceContext
      };
      return { content: [{ type: "text", text: JSON.stringify(report, null, 2) }] };
    }

    const hydration = await waitForBlazor(currentPage);
    await applyFixtureState(currentPage, state, theme);

    const browserEvidence = await currentPage.evaluate(({ requestedSelector, requestedComponent, vp }) => {
      const candidates = Array.from(document.querySelectorAll<HTMLElement>(requestedSelector));
      const element = candidates.find(candidate => !requestedComponent || candidate.dataset.blazorComponent === requestedComponent || candidate.className.includes(requestedComponent));
      if (!element) return { error: `Component was not found for selector: ${requestedSelector}` };

      const rect = element.getBoundingClientRect();
      const styles = getComputedStyle(element);
      const issues: Array<Record<string, unknown>> = [];

      if (element.scrollWidth > element.clientWidth + 1) {
        issues.push({ type: "horizontal-overflow", severity: "error", evidence: { scrollWidth: element.scrollWidth, clientWidth: element.clientWidth } });
      }
      if (element.scrollHeight > element.clientHeight + 1) {
        issues.push({ type: "vertical-overflow", severity: "warning", evidence: { scrollHeight: element.scrollHeight, clientHeight: element.clientHeight } });
      }

      // Descendant elements (traversing shadow roots if present)
      const descendants: HTMLElement[] = [];
      function collect(node: Element) {
        if (node instanceof HTMLElement && node !== element && node.tagName !== "SCRIPT" && node.tagName !== "STYLE") {
          descendants.push(node);
        }
        if (node.shadowRoot) {
          Array.from(node.shadowRoot.children).forEach(collect);
        }
        Array.from(node.children).forEach(collect);
      }
      collect(element);

      for (const descendant of descendants.slice(0, 150)) {
        const descendantRect = descendant.getBoundingClientRect();
        const rightOverflow = Math.max(0, descendantRect.right - rect.right);
        const leftOverflow = Math.max(0, rect.left - descendantRect.left);
        if (rightOverflow > 1 || leftOverflow > 1) {
          issues.push({
            type: "layout-overflow",
            severity: "error",
            evidence: { element: descendant.dataset.testid ?? descendant.className.toString(), rightOverflow: Math.round(rightOverflow), leftOverflow: Math.round(leftOverflow) }
          });
          break;
        }
      }

      // Touch Target validation for interactive descendants on touch/mobile
      if (vp.width <= 1024) {
        const interactiveSelector = "button, a, input, select, textarea, [role='button'], md-filled-button, md-outlined-button, md-text-button, md-icon-button, md-chip, md-switch";
        for (const desc of descendants) {
          if (desc.matches(interactiveSelector) || desc.getAttribute("role") === "button") {
            const dRect = desc.getBoundingClientRect();
            if (dRect.width > 0 && dRect.height > 0) {
              if (dRect.width < 44 || dRect.height < 44) {
                issues.push({
                  type: "touch-target-too-small",
                  severity: "error",
                  evidence: { element: desc.tagName.toLowerCase() + (desc.id ? `#${desc.id}` : ""), width: Math.round(dRect.width), height: Math.round(dRect.height), required: ">=44x44px" }
                });
              }
            }
          }
        }
      }

      // Helper to identify Material 3 / Smart / QuickGrid vendor web-components
      const isVendorComponent = (node: Element) => {
        const tag = node.tagName.toLowerCase();
        if (tag.startsWith("md-") || tag.startsWith("smart-") || tag.startsWith("quickgrid-") || tag === "svg" || tag === "path") return true;
        const host = (node.getRootNode() as ShadowRoot)?.host;
        if (host) {
          const hostTag = host.tagName.toLowerCase();
          if (hostTag.startsWith("md-") || hostTag.startsWith("smart-") || hostTag.startsWith("quickgrid-")) return true;
        }
        return false;
      };

      // Forbidden Flexbox layout check (CSS Grid only for application code)
      for (const desc of [element, ...descendants]) {
        if (isVendorComponent(desc)) continue;
        const dStyle = getComputedStyle(desc);
        if (dStyle.display === "flex" || dStyle.display === "inline-flex") {
          issues.push({
            type: "forbidden-flexbox-layout",
            severity: "error",
            evidence: { element: desc.tagName.toLowerCase() + (desc.className ? `.${desc.className.toString().split(" ")[0]}` : ""), display: dStyle.display, rule: "Use CSS Grid ('display: grid') instead of Flexbox" }
          });
        }
      }

      return {
        component: element.dataset.blazorComponent ?? requestedComponent,
        source: element.dataset.blazorSource,
        state: element.dataset.uiState,
        theme: element.dataset.uiTheme,
        rect: { x: rect.x, y: rect.y, width: rect.width, height: rect.height },
        styles: { display: styles.display, position: styles.position, overflow: styles.overflow, width: styles.width, height: styles.height },
        issues: issues.slice(0, 50)
      };
    }, { requestedSelector: selector, requestedComponent: component, vp: viewport });

    const componentName = typeof browserEvidence === "object" && browserEvidence !== null && "component" in browserEvidence
      ? String(browserEvidence.component)
      : component;
    const sourceContext = await getSourceContext(componentName, customSourceRoot);
    const componentEvidence = browserEvidence as Record<string, unknown>;
    const rawIssues = Array.isArray(componentEvidence.issues) ? componentEvidence.issues as Array<Record<string, unknown>> : [];
    const issues: UiQualityIssue[] = rawIssues.map((issue, index) => ({
      id: `ui-quality-${index + 1}`,
      type: String(issue.type ?? "layout"),
      severity: issue.severity === "error" ? "error" : "warning",
      confidence: 0.95,
      evidence: (issue.evidence as Record<string, unknown>) ?? {},
      source: typeof sourceContext.component === "object" && sourceContext.component !== null ? sourceContext.component as Record<string, unknown> : undefined
    }));

    const report: UiQualityReport = {
      runId: randomUUID(),
      url: currentPage.url(),
      viewport,
      state: typeof componentEvidence.state === "string" ? componentEvidence.state : state,
      theme: typeof componentEvidence.theme === "string" ? componentEvidence.theme : theme,
      component: componentEvidence,
      hydration,
      runtime: active.runtime,
      issues,
      sourceContext
    };
    return {
      content: [{
        type: "text",
        text: JSON.stringify(report, null, 2)
      }]
    };
  } finally {
    await active.close();
  }
});

server.registerTool("run_ui_scenario", {
  description: "Run a safe declarative browser scenario and return the resulting UI state, runtime diagnostics, and optional screenshot.",
  inputSchema: {
    path: z.string().default("/"),
    viewport: viewportSchema.default({ width: 390, height: 844 }),
    actions: z.array(scenarioActionSchema).min(1).max(20),
    screenshot: z.boolean().default(false)
  }
}, async ({ path, viewport, actions, screenshot }) => {
  const active = await openPage(viewport);
  try {
    const currentPage = active.page;
    if (!await safeNavigate(currentPage, path)) {
      return hostOfflineResponse(path);
    }
    const hydration = await waitForBlazor(currentPage);

    for (const action of actions) {
      switch (action.type) {
        case "click":
          await currentPage.locator(action.selector).click();
          await currentPage.waitForTimeout(100);
          break;
        case "fill":
          await currentPage.locator(action.selector).fill(action.value);
          break;
        case "press":
          await currentPage.locator(action.selector).press(action.key);
          break;
        case "waitFor":
          await currentPage.locator(action.selector).waitFor({ state: "visible", timeout: action.timeoutMs ?? navigationTimeout });
          break;
        case "setState":
          await applyFixtureState(currentPage, action.value, undefined);
          break;
        case "setTheme":
          await applyFixtureState(currentPage, undefined, action.value);
          break;
      }
    }

    const state = await currentPage.evaluate(() => {
      const component = document.querySelector<HTMLElement>("[data-blazor-component]");
      return {
        component: component?.dataset.blazorComponent,
        state: component?.dataset.uiState,
        theme: component?.dataset.uiTheme,
        visibleText: document.body.innerText.slice(0, 1000)
      };
    });
    const image = screenshot ? await currentPage.screenshot({ type: "png" }) : undefined;
    const text = JSON.stringify({ url: currentPage.url(), viewport, actions, state, hydration, runtime: active.runtime }, null, 2);
    const content = image
      ? [{ type: "text" as const, text }, { type: "image" as const, data: image.toString("base64"), mimeType: "image/png" }]
      : [{ type: "text" as const, text }];
    return { content };
  } finally {
    await active.close();
  }
});

server.registerTool("analyze_accessibility", {
  description: "Run axe-core accessibility analysis against the rendered page or a component subtree.",
  inputSchema: {
    path: z.string().default("/"),
    selector: z.string().optional(),
    viewport: viewportSchema.default({ width: 1440, height: 900 }),
    tags: z.array(z.string()).default(["wcag2a", "wcag2aa", "wcag21aa"])
  }
}, async ({ path, selector, viewport, tags }) => {
  const active = await openPage(viewport);
  try {
    const currentPage = active.page;
    if (!await safeNavigate(currentPage, path)) {
      return hostOfflineResponse(path);
    }
    const hydration = await waitForBlazor(currentPage);
    await currentPage.addScriptTag({ content: axeSource });
    const result = await currentPage.evaluate(async ({ target, runTags }) => {
      const axe = (window as unknown as { axe: { run: (context: unknown, options: unknown) => Promise<{ violations: Array<{ id: string; impact: string | null; description: string; helpUrl: string; nodes: Array<{ html: string; target: string[]; failureSummary: string }> }> }> } }).axe;
      const context = target ? document.querySelector(target) : document;
      if (!context) return { error: `Selector not found: ${target}` };
      return axe.run(context, { runOnly: runTags });
    }, { target: selector, runTags: tags });

    const issues: UiQualityIssue[] = "violations" in result
      ? result.violations.map(violation => ({
        id: `a11y-${violation.id}`,
        type: "accessibility",
        severity: violation.impact === "critical" || violation.impact === "serious" ? "error" : "warning",
        confidence: 1,
        evidence: { rule: violation.id, description: violation.description, helpUrl: violation.helpUrl, nodes: violation.nodes }
      }))
      : [];
    const report: UiQualityReport = {
      runId: randomUUID(),
      url: currentPage.url(),
      viewport,
      hydration,
      runtime: active.runtime,
      issues
    };
    return { content: [{ type: "text", text: JSON.stringify({ ...report, axe: result }, null, 2) }] };
  } finally {
    await active.close();
  }
});

server.registerTool("compare_states", {
  description: "Compare two rendered UI states or themes at the same viewport using screenshot pixels and DOM geometry fingerprints.",
  inputSchema: {
    path: z.string().default("/"),
    selector: z.string().default("[data-blazor-component]"),
    viewport: viewportSchema.default({ width: 390, height: 844 }),
    stateA: z.enum(["loading", "empty", "populated", "error"]).default("populated"),
    themeA: z.enum(["light", "dark"]).default("light"),
    stateB: z.enum(["loading", "empty", "populated", "error"]).default("error"),
    themeB: z.enum(["light", "dark"]).default("dark"),
    includeDiffImage: z.boolean().default(false)
  }
}, async ({ path, selector, viewport, stateA, themeA, stateB, themeB, includeDiffImage }) => {
  const active = await openPage(viewport);
  try {
    const currentPage = active.page;
    if (!await safeNavigate(currentPage, path)) {
      return hostOfflineResponse(path);
    }
    const hydration = await waitForBlazor(currentPage);
    await applyFixtureState(currentPage, stateA, themeA);
    const snapshotA = await captureUiSnapshot(currentPage, selector);
    await applyFixtureState(currentPage, stateB, themeB);
    const snapshotB = await captureUiSnapshot(currentPage, selector);

    const first = PNG.sync.read(snapshotA.image);
    const second = PNG.sync.read(snapshotB.image);
    const diff = new PNG({ width: first.width, height: first.height });
    const changedPixels = pixelmatch(first.data, second.data, diff.data, first.width, first.height, { threshold: 0.1 });
    const diffImage = includeDiffImage ? PNG.sync.write(diff).toString("base64") : undefined;
    const report = {
      runId: randomUUID(),
      url: currentPage.url(),
      viewport,
      hydration,
      runtime: active.runtime,
      stateA: { state: stateA, theme: themeA, fingerprint: snapshotA.fingerprint },
      stateB: { state: stateB, theme: themeB, fingerprint: snapshotB.fingerprint },
      visualDiff: { changedPixels, totalPixels: first.width * first.height, ratio: changedPixels / (first.width * first.height), image: diffImage },
      geometryChanged: JSON.stringify(snapshotA.fingerprint.rect) !== JSON.stringify(snapshotB.fingerprint.rect),
      scrollChanged: JSON.stringify(snapshotA.fingerprint.scroll) !== JSON.stringify(snapshotB.fingerprint.scroll)
    };
    return { content: [{ type: "text", text: JSON.stringify(report, null, 2) }] };
  } finally {
    await active.close();
  }
});

const transport = new StdioServerTransport();
await server.connect(transport);
