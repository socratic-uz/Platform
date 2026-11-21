import fs from 'fs';
import path from 'path';
import os from 'os';
import { fileURLToPath, pathToFileURL } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const assetsDir = path.resolve(__dirname, '..');
const wwwrootDir = path.resolve(assetsDir, 'wwwroot');
const frontendDir = path.resolve(assetsDir, '../../../..');
const indexPath = path.resolve(wwwrootDir, 'index.html');
const analyticsDir = path.resolve(wwwrootDir, 'analytics');
const blueprintsDir = path.resolve(analyticsDir, 'blueprints');

[analyticsDir, blueprintsDir].forEach((dir) => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
});

const VIEWPORTS = [
  { name: 'desktop', width: 1440, height: 900 },
  { name: 'tablet', width: 820, height: 1180 },
  { name: 'mobile', width: 390, height: 844 }
];

const ROUTE_TO_NAME = {
  '/': 'landing',
  '/signin-google-callback': 'signin-google',
  '/signin-telegram-callback': 'signin-telegram'
};

function discoverStaticRoutes() {
  const routes = new Set();
  function walk(dir) {
    if (!fs.existsSync(dir)) return;
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const full = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        if (entry.name !== 'bin' && entry.name !== 'obj' && entry.name !== 'node_modules' && entry.name !== '.git') {
          walk(full);
        }
      } else if (entry.isFile() && entry.name.endsWith('.razor.cs')) {
        const content = fs.readFileSync(full, 'utf8');
        for (const m of content.matchAll(/\[Route\("([^"]+)"\)\]/g)) {
          const r = m[1];
          if (!r.includes('{') && r !== '/Error' && r !== '/data') {
            routes.add(r);
          }
        }
      }
    }
  }
  walk(frontendDir);

  return [...routes]
    .sort()
    .map((route) => ({
      route,
      name: ROUTE_TO_NAME[route] || route.replace(/^\/+/, '').replace(/\//g, '-')
    }));
}

async function getPlaywright() {
  const candidatePaths = [
    path.resolve(__dirname, '../../../../Tools/ui-mcp/node_modules/playwright/index.mjs'),
    path.resolve(__dirname, '../node_modules/playwright/index.mjs')
  ];

  for (const playwrightPath of candidatePaths) {
    if (fs.existsSync(playwrightPath)) {
      try {
        const { chromium } = await import(pathToFileURL(playwrightPath).href);
        return chromium;
      } catch {
        // Continue checking candidates
      }
    }
  }
  return null;
}

let cachedCollectorCode = null;
function getCollectorCode() {
  if (cachedCollectorCode === null) {
    const collectorScriptPath = path.resolve(__dirname, 'layout-analyzer.js');
    cachedCollectorCode = fs.existsSync(collectorScriptPath) ? fs.readFileSync(collectorScriptPath, 'utf8') : '';
  }
  return cachedCollectorCode;
}

async function createBootedWasmPage(browser) {
  const context = await browser.newContext({
    deviceScaleFactor: 1,
    reducedMotion: 'reduce'
  });

  // Abort external HTTP/HTTPS calls (Google Fonts, localhost:5000 gRPC, etc.) for fast deterministic layouts
  await context.route('**/*', (route) => {
    const url = route.request().url();
    if (url.startsWith('http:') || url.startsWith('https:')) {
      return route.abort();
    }
    return route.continue();
  });

  const page = await context.newPage();
  const entryUrl = `${pathToFileURL(indexPath).href}#/`;
  await page.goto(entryUrl, { waitUntil: 'load' });
  await page.waitForFunction(
    () => Boolean(document.querySelector('body > header, body > main, body > aside, body > section')),
    null,
    { timeout: 30000 }
  );
  await page.addStyleTag({ content: '*, *::before, *::after { transition: none !important; animation: none !important; }' });

  const collectorCode = getCollectorCode();
  if (collectorCode) {
    await page.evaluate(collectorCode);
  }
  return { context, page };
}

async function auditRouteOnBootedPage(page, route) {
  await page.evaluate((r) => {
    location.hash = '#' + r;
  }, route);

  await page.evaluate(async () => {
    await new Promise((r) => setTimeout(r, 80));
    await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve)));
    if (window.customElements && window.customElements.whenDefined) {
      await Promise.race([
        window.customElements.whenDefined('md-filled-button'),
        new Promise((r) => setTimeout(r, 100))
      ]);
    }
  });

  const results = {};
  for (const vp of VIEWPORTS) {
    await page.setViewportSize({ width: vp.width, height: vp.height });
    await page.evaluate(() => new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve))));

    const res = await page.evaluate((viewport) => {
      if (window.LayoutAnalyzer) {
        const auditRes = window.LayoutAnalyzer.audit('body');
        return {
          layout: `viewport [0,0,${viewport.width},${viewport.height}] [0,0,0,0]\n└── ` + auditRes.layout,
          violations: auditRes.violations
        };
      }
      return null;
    }, vp);
    results[vp.name] = res;
  }
  return results;
}

function writePageBlueprints(pageName, results) {
  let totalViols = 0;
  for (const vp of VIEWPORTS) {
    const result = results ? results[vp.name] : null;
    if (result && result.layout) {
      fs.writeFileSync(path.resolve(blueprintsDir, `${pageName}.${vp.name}.blueprint`), result.layout, 'utf8');
    }
    if (result && result.violations) {
      totalViols += result.violations.length;
      for (const v of result.violations) {
        const icon = v.type === 'SiblingCollision' ? '⚠️' : (v.type === 'BoundaryOverflow' ? '❌' : 'ℹ️');
        console.log(`  ${icon} [${pageName}][${vp.name}] ${v.type}: ${v.path || v.target} - ${v.message}`);
      }
    }
  }
  return totalViols;
}

async function runBatchAnalysis(browser, isQuiet = false) {
  const startTime = performance.now();
  if (!fs.existsSync(indexPath)) {
    throw new Error(`Standalone WASM index.html not found at ${indexPath}. Build Web.UI.Client first.`);
  }

  const pageArgIdx = process.argv.indexOf('--page');
  const targetPage = pageArgIdx !== -1 && process.argv[pageArgIdx + 1] ? process.argv[pageArgIdx + 1] : null;

  let routes = discoverStaticRoutes();
  if (targetPage) {
    routes = routes.filter((r) => r.name === targetPage);
  }
  if (routes.length === 0) return;

  const manifest = {
    generatedAt: new Date().toISOString(),
    pagesCount: routes.length,
    pages: []
  };

  let totalViolations = 0;

  if (browser) {
    // 2 booted WASM workers share the route queue (avoids launching 6x 43MB WASM runtimes simultaneously)
    const concurrency = Math.min(2, routes.length);
    const queue = [...routes];

    async function worker() {
      const { context, page } = await createBootedWasmPage(browser);
      try {
        while (queue.length > 0) {
          const item = queue.shift();
          if (!item) break;
          const results = await auditRouteOnBootedPage(page, item.route);
          const totalViols = writePageBlueprints(item.name, results);
          totalViolations += totalViols;
          manifest.pages.push({
            name: item.name,
            file: `index.html#${item.route}`,
            blueprintFiles: {
              desktop: `analytics/blueprints/${item.name}.desktop.blueprint`,
              tablet: `analytics/blueprints/${item.name}.tablet.blueprint`,
              mobile: `analytics/blueprints/${item.name}.mobile.blueprint`
            },
            totalViolations: totalViols
          });
        }
      } finally {
        await context.close();
      }
    }

    await Promise.all(Array.from({ length: concurrency }, () => worker()));
  } else {
    for (const item of routes) {
      manifest.pages.push({
        name: item.name,
        file: `index.html#${item.route}`,
        blueprintFiles: {
          desktop: `analytics/blueprints/${item.name}.desktop.blueprint`,
          tablet: `analytics/blueprints/${item.name}.tablet.blueprint`,
          mobile: `analytics/blueprints/${item.name}.mobile.blueprint`
        },
        totalViolations: 0
      });
    }
  }

  manifest.pages.sort((a, b) => a.name.localeCompare(b.name));
  fs.writeFileSync(path.resolve(analyticsDir, 'manifest.json'), JSON.stringify(manifest, null, 2), 'utf8');

  const totalElapsed = Math.round(performance.now() - startTime);
  if (!isQuiet) {
    console.log(`\n✨ Batch layout analysis completed in ${totalElapsed}ms across ${routes.length} routes via index.html#/*. Total violations: ${totalViolations}`);
  }
}

async function startWatchMode(browser) {
  console.log(`\n👀 [AI Layout Analyzer • Fast Watch Mode Active]`);
  console.log(`   Watching for changes in: wwwroot/css/ and wwwroot/_framework/`);

  let debounceTimer = null;
  const watchDirs = [path.resolve(wwwrootDir, 'css'), path.resolve(wwwrootDir, '_framework')];
  watchDirs.forEach((dir) => {
    if (!fs.existsSync(dir)) return;
    fs.watch(dir, { recursive: true }, (_eventType, filename) => {
      if (!filename) return;
      clearTimeout(debounceTimer);
      debounceTimer = setTimeout(async () => {
        const timeStr = new Date().toLocaleTimeString();
        console.log(`[${timeStr}] 🎨 Asset updated: ${filename}. Re-evaluating all index.html#* geometries...`);
        await runBatchAnalysis(browser, true);
      }, 250);
    });
  });
}

async function main() {
  const isWatch = process.argv.includes('--watch') || process.argv.includes('-w');
  const staticOnly = process.argv.includes('--static-only');
  const chromium = staticOnly ? null : await getPlaywright();
  let browser = null;
  if (chromium) {
    browser = await chromium.launch({ headless: true, args: ['--allow-file-access-from-files'] });
  } else if (!staticOnly) {
    console.warn('⚠️ Playwright not found. Skipping live browser audit.');
  }

  try {
    if (isWatch) {
      await runBatchAnalysis(browser, false);
      await startWatchMode(browser);
    } else {
      console.log('🚀 [Socratic AI Layout Analyzer] Running audit via index.html#*...');
      await runBatchAnalysis(browser, false);
    }
  } finally {
    if (!isWatch && browser) {
      await browser.close();
    }
  }
}

main().catch((err) => {
  console.error('Fatal error in layout analyzer:', err);
  process.exit(1);
});
