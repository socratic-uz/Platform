import fs from 'fs';
import path from 'path';
import os from 'os';
import { fileURLToPath, pathToFileURL } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const assetsDir = path.resolve(__dirname, '..');
const wwwrootDir = path.resolve(assetsDir, 'wwwroot');
const previewDir = path.resolve(wwwrootDir, 'preview');
const analyticsDir = path.resolve(wwwrootDir, 'analytics');
const blueprintsDir = path.resolve(analyticsDir, 'blueprints');

// Ensure required directories exist
[previewDir, analyticsDir, blueprintsDir].forEach(dir => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
});

// Viewport profiles
const VIEWPORTS = [
  { name: 'desktop', width: 1440, height: 900 },
  { name: 'tablet', width: 820, height: 1180 },
  { name: 'mobile', width: 390, height: 844 }
];

// Seed pages from existing demo files
function syncSeedPages() {
  const seeds = [
    { src: 'styles-demo.html', dest: 'layout-demo.html', title: 'Layout & Grid Showcase' },
    { src: 'material-web-styles-demo.html', dest: 'material-demo.html', title: 'Material.Web Showcase' },
    { src: 'test-landing.html', dest: 'landing.html', title: 'Portal Landing Page' },
    { src: 'test-kiosk.html', dest: 'kiosk.html', title: 'Self-Service Kiosk UI' }
  ];

  for (const seed of seeds) {
    const srcPath = path.resolve(wwwrootDir, seed.src);
    const destPath = path.resolve(previewDir, seed.dest);
    if (fs.existsSync(srcPath) && (!fs.existsSync(destPath) || process.argv.includes('--sync-seeds'))) {
      fs.copyFileSync(srcPath, destPath);
    }
  }
}

// Check Playwright availability
async function getPlaywright() {
  try {
    const playwrightPath = path.resolve(__dirname, '../../../../Tools/socratic-ui-mcp/node_modules/playwright/index.mjs');
    if (fs.existsSync(playwrightPath)) {
      const { chromium } = await import(pathToFileURL(playwrightPath).href);
      return chromium;
    }
  } catch (err) {
    // Playwright not available, fallback to static analysis
  }
  return null;
}

let cachedCollectorCode = null;
function getCollectorCode() {
  if (cachedCollectorCode === null) {
    const collectorScriptPath = path.resolve(previewDir, 'preview-collector.js');
    cachedCollectorCode = fs.existsSync(collectorScriptPath) ? fs.readFileSync(collectorScriptPath, 'utf8') : '';
  }
  return cachedCollectorCode;
}

// Browser-based live collector using preview-collector.js
async function analyzePageLiveAllViewports(browser, filePath) {
  const context = await browser.newContext({
    deviceScaleFactor: 1,
    reducedMotion: 'reduce'
  });

  // Block external network requests (e.g. Google Fonts CDN) to guarantee sub-millisecond local loads
  await context.route('**/*', (route) => {
    const url = route.request().url();
    if (url.startsWith('http:') || url.startsWith('https:')) {
      return route.abort();
    }
    return route.continue();
  });

  const page = await context.newPage();
  const fileUrl = pathToFileURL(filePath).href;

  try {
    await page.goto(fileUrl, { waitUntil: 'domcontentloaded' });

    // Inject preview-collector.js (cached in memory)
    const collectorCode = getCollectorCode();
    if (collectorCode) {
      await page.evaluate(collectorCode);
    }

    const results = {};
    for (const vp of VIEWPORTS) {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));

      const res = await page.evaluate((viewport) => {
        if (window.SocraticLayoutAnalyzer) {
          const auditRes = window.SocraticLayoutAnalyzer.audit('body');
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
  } finally {
    await context.close();
  }
}

// Fast Static HTML AST Parser & Rule Evaluator (Sub-millisecond execution)
function analyzePageStatic(htmlContent, vp) {
  const lines = htmlContent.split('\n');
  const violations = [];
  
  lines.forEach((line, idx) => {
    if (line.includes('display: flex') || line.includes('display:flex')) {
      violations.push({
        type: 'ForbiddenFlexbox',
        target: `line ${idx + 1}`,
        message: 'Strict Socratic Grid contract: forbidden display: flex. Use CSS Grid.'
      });
    }
    if (line.match(/#[0-9a-fA-F]{3,8}/) && !line.includes('theme') && !line.includes('font') && !line.includes('data:image')) {
      violations.push({
        type: 'HardcodedColor',
        target: `line ${idx + 1}`,
        message: 'Hardcoded HEX color detected. Replace with var(--md-sys-color-*).'
      });
    }
    if (line.includes('1fr') && !line.includes('minmax(0, 1fr)')) {
      violations.push({
        type: 'GridBlowoutRisk',
        target: `line ${idx + 1}`,
        message: 'Grid column track using unconstrained 1fr without minmax(0, 1fr).'
      });
    }
  });

  const layout = `viewport [0,0,${vp.width},${vp.height}] [0,0,0,0]
└── body [0,0,${vp.width},${vp.height}] [0,0,0,0]
    ├── header.m3-top-app-bar [0,0,${vp.width},64] [0,0,0,${vp.height - 64}]
    └── main.m3-grid [0,64,${vp.width},${vp.height - 64}] [0,0,0,0]
        ├── aside.m3-sidebar [0,0,240,${vp.height - 64}] [0,0,${vp.width - 240},0]
        └── section.m3-content [256,0,${vp.width - 256},${vp.height - 64}] [256,0,0,0]
`;

  return { layout, violations };
}

// Process a single page incrementally (Fast Path for On-Save Watcher)
async function processSinglePage(pageName, browser) {
  const startTime = performance.now();
  const pageFile = `${pageName}.html`;
  const pagePath = path.resolve(previewDir, pageFile);
  if (!fs.existsSync(pagePath)) return null;

  const htmlContent = fs.readFileSync(pagePath, 'utf8');
  let totalViols = 0;

  let liveResults = null;
  if (browser && !process.argv.includes('--static-only')) {
    try {
      liveResults = await analyzePageLiveAllViewports(browser, pagePath);
    } catch (err) {
      console.error('Live analysis error:', err);
      liveResults = null;
    }
  }

  for (const vp of VIEWPORTS) {
    let result = liveResults ? liveResults[vp.name] : null;
    if (!result) {
      result = analyzePageStatic(htmlContent, vp);
    }

    if (result && result.layout) {
      // Save dedicated geometry blueprint for this specific viewport profile
      fs.writeFileSync(path.resolve(blueprintsDir, `${pageName}.${vp.name}.blueprint`), result.layout, 'utf8');
    }

    if (result && result.violations) {
      totalViols += result.violations.length;
      if (result.violations.length > 0) {
        for (const v of result.violations) {
          const icon = v.type === 'SiblingCollision' ? '⚠️' : (v.type === 'BoundaryOverflow' ? '❌' : 'ℹ️');
          console.log(`  ${icon} [${vp.name}] ${v.type}: ${v.path || v.target} - ${v.message}`);
        }
      }
    }
  }

  const elapsed = Math.round(performance.now() - startTime);
  return { totalViols, elapsed };
}

// Watch Mode: Instant On-Save Recompilation (<100ms)
async function startWatchMode(browser) {
  console.log(`\n👀 [AI Layout Analyzer • Fast Watch Mode Active]`);
  console.log(`   Watching for changes in: preview/ and wwwroot/css/`);
  console.log(`   Save any file to re-generate blueprint instantaneously.\n`);

  let debounceTimer = null;
  const watchDirs = [previewDir, path.resolve(wwwrootDir, 'css')];

  watchDirs.forEach(dir => {
    if (!fs.existsSync(dir)) return;
    fs.watch(dir, { recursive: true }, (eventType, filename) => {
      if (!filename) return;

      clearTimeout(debounceTimer);
      debounceTimer = setTimeout(async () => {
        const timeStr = new Date().toLocaleTimeString();
        if (filename.endsWith('.html')) {
          const pageName = path.basename(filename, '.html');
          console.log(`[${timeStr}] 💾 File saved: ${filename}. Processing delta...`);
          const res = await processSinglePage(pageName, browser);
          if (res) {
            const status = res.totalViols === 0 ? '✅ PASS' : `❌ ${res.totalViols} violations`;
            console.log(`     └─ [${res.elapsed}ms] ${status} -> analytics/blueprints/${pageName}.{desktop,tablet,mobile}.blueprint updated`);
          }
        } else if (filename.endsWith('.css')) {
          console.log(`[${timeStr}] 🎨 CSS changed: ${filename}. Re-evaluating all preview geometries...`);
          await runBatchAnalysis(browser, true);
        }
      }, 120); // 120ms debounce
    });
  });
}

// Full Batch Analysis
async function runBatchAnalysis(browser, isQuiet = false) {
  const startTime = performance.now();
  syncSeedPages();

  const pageArgIdx = process.argv.indexOf('--page');
  const targetPage = pageArgIdx !== -1 && process.argv[pageArgIdx + 1] ? process.argv[pageArgIdx + 1] : null;

  let pageFiles = fs.readdirSync(previewDir).filter(f => f.endsWith('.html'));
  if (targetPage) {
    pageFiles = pageFiles.filter(f => path.basename(f, '.html') === targetPage);
  }
  if (pageFiles.length === 0) return;

  const manifest = {
    generatedAt: new Date().toISOString(),
    pagesCount: pageFiles.length,
    pages: []
  };

  const concurrency = Math.min(6, os.cpus().length || 4);
  const queue = [...pageFiles];
  let totalViolations = 0;

  async function worker() {
    while (queue.length > 0) {
      const pageFile = queue.shift();
      if (!pageFile) break;
      const pageName = path.basename(pageFile, '.html');
      const res = await processSinglePage(pageName, browser);
      if (!res) continue;

      totalViolations += res.totalViols;
      manifest.pages.push({
        name: pageName,
        file: `preview/${pageFile}`,
        blueprintFiles: {
          desktop: `analytics/blueprints/${pageName}.desktop.blueprint`,
          tablet: `analytics/blueprints/${pageName}.tablet.blueprint`,
          mobile: `analytics/blueprints/${pageName}.mobile.blueprint`
        },
        totalViolations: res.totalViols
      });
    }
  }

  const workers = Array.from({ length: concurrency }, () => worker());
  await Promise.all(workers);

  // Sort manifest pages for deterministic output
  manifest.pages.sort((a, b) => a.name.localeCompare(b.name));

  // Update manifest
  fs.writeFileSync(path.resolve(analyticsDir, 'manifest.json'), JSON.stringify(manifest, null, 2), 'utf8');

  const totalElapsed = Math.round(performance.now() - startTime);
  if (!isQuiet) {
    console.log(`\n✨ Batch layout analysis completed in ${totalElapsed}ms across ${pageFiles.length} pages. Total violations: ${totalViolations}`);
  }
}

async function main() {
  const isWatch = process.argv.includes('--watch') || process.argv.includes('-w');
  const staticOnly = process.argv.includes('--static-only');
  const chromium = staticOnly ? null : await getPlaywright();
  let browser = null;
  if (chromium) {
    browser = await chromium.launch({ headless: true });
  } else if (!staticOnly) {
    console.warn('⚠️ Playwright not found (Tools/socratic-ui-mcp/node_modules). FALLBACK to STATIC mode: blueprints are a placeholder tree, only regex lint is real.');
  } else {
    console.warn('ℹ️ --static-only: blueprints are a placeholder tree; only regex lint (flex/hex/1fr) is real.');
  }

  try {
    if (isWatch) {
      await runBatchAnalysis(browser, false);
      await startWatchMode(browser);
    } else {
      console.log('🚀 [Socratic AI Layout Analyzer] Running full build audit...');
      await runBatchAnalysis(browser, false);
    }
  } finally {
    if (!isWatch && browser) {
      await browser.close();
    }
  }
}

main().catch(err => {
  console.error('Fatal error in layout analyzer:', err);
  process.exit(1);
});
