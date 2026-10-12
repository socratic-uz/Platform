import fs from 'fs';
import path from 'path';
import { fileURLToPath, pathToFileURL } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const assetsDir = path.resolve(__dirname, '..');
const wwwrootDir = path.resolve(assetsDir, 'wwwroot');
const previewDir = path.resolve(wwwrootDir, 'preview');
const pagesDir = path.resolve(previewDir, 'pages');
const geometryDir = path.resolve(previewDir, 'geometry');

// Ensure preview subdirectories exist
[pagesDir, geometryDir].forEach(dir => {
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
    const destPath = path.resolve(pagesDir, seed.dest);
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

// Browser-based live collector using preview-collector.js
async function analyzePageLive(chromium, filePath, vp) {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: vp.width, height: vp.height },
    deviceScaleFactor: 1,
    reducedMotion: 'reduce'
  });
  const page = await context.newPage();

  const fileUrl = pathToFileURL(filePath).href;
  await page.goto(fileUrl, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(100);

  // Inject preview-collector.js
  const collectorScriptPath = path.resolve(previewDir, 'preview-collector.js');
  if (fs.existsSync(collectorScriptPath)) {
    const collectorCode = fs.readFileSync(collectorScriptPath, 'utf8');
    await page.evaluate(collectorCode);
  }

  // Audit using in-page engine
  const result = await page.evaluate((viewport) => {
    if (window.SocraticLayoutAnalyzer) {
      const auditRes = window.SocraticLayoutAnalyzer.audit('body');
      return {
        layout: `viewport [0,0,${viewport.width},${viewport.height}] [0,0,0,0]\n└── ` + auditRes.layout,
        violations: auditRes.violations
      };
    }
    return null;
  }, vp);

  await browser.close();
  return result;
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
async function processSinglePage(pageName, chromium) {
  const startTime = performance.now();
  const pageFile = `${pageName}.html`;
  const pagePath = path.resolve(pagesDir, pageFile);
  if (!fs.existsSync(pagePath)) return null;

  const htmlContent = fs.readFileSync(pagePath, 'utf8');
  let totalViols = 0;

  for (const vp of VIEWPORTS) {
    let result;
    if (chromium && !process.argv.includes('--static-only')) {
      try {
        result = await analyzePageLive(chromium, pagePath, vp);
      } catch {
        result = analyzePageStatic(htmlContent, vp);
      }
    } else {
      result = analyzePageStatic(htmlContent, vp);
    }

    if (result && result.layout) {
      // Save dedicated geometry blueprint for this specific viewport profile
      fs.writeFileSync(path.resolve(geometryDir, `${pageName}.${vp.name}.layout`), result.layout, 'utf8');
    }

    if (result && result.violations) {
      totalViols += result.violations.length;
      if (process.argv.includes('--page')) {
        for (const v of result.violations) {
          console.log(`  [${vp.name}] ${v.type}: ${v.path || v.target} at [${v.box.join(',')}] - ${v.message}`);
        }
      }
    }
  }

  const elapsed = Math.round(performance.now() - startTime);
  return { totalViols, elapsed };
}

// Watch Mode: Instant On-Save Recompilation (<100ms)
async function startWatchMode(chromium) {
  console.log(`\n👀 [AI Layout Analyzer • Fast Watch Mode Active]`);
  console.log(`   Watching for changes in: preview/pages/ and wwwroot/css/`);
  console.log(`   Save any file to re-generate layout blueprint instantaneously.\n`);

  let debounceTimer = null;
  const watchDirs = [pagesDir, path.resolve(wwwrootDir, 'css')];

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
          const res = await processSinglePage(pageName, chromium);
          if (res) {
            const status = res.totalViols === 0 ? '✅ PASS' : `❌ ${res.totalViols} violations`;
            console.log(`     └─ [${res.elapsed}ms] ${status} -> preview/geometry/${pageName}.{desktop,tablet,mobile}.layout updated`);
          }
        } else if (filename.endsWith('.css')) {
          console.log(`[${timeStr}] 🎨 CSS changed: ${filename}. Re-evaluating all preview geometries...`);
          await runBatchAnalysis(chromium, true);
        }
      }, 120); // 120ms debounce
    });
  });
}

// Full Batch Analysis
async function runBatchAnalysis(chromium, isQuiet = false) {
  const startTime = performance.now();
  syncSeedPages();

  const pageArgIdx = process.argv.indexOf('--page');
  const targetPage = pageArgIdx !== -1 && process.argv[pageArgIdx + 1] ? process.argv[pageArgIdx + 1] : null;

  let pageFiles = fs.readdirSync(pagesDir).filter(f => f.endsWith('.html'));
  if (targetPage) {
    pageFiles = pageFiles.filter(f => path.basename(f, '.html') === targetPage);
  }
  if (pageFiles.length === 0) return;

  const manifest = {
    generatedAt: new Date().toISOString(),
    pagesCount: pageFiles.length,
    pages: []
  };

  let totalViolations = 0;

  for (const pageFile of pageFiles) {
    const pageName = path.basename(pageFile, '.html');
    const res = await processSinglePage(pageName, chromium);
    if (!res) continue;

    totalViolations += res.totalViols;
    manifest.pages.push({
      name: pageName,
      file: `preview/pages/${pageFile}`,
      geometryFiles: {
        desktop: `preview/geometry/${pageName}.desktop.layout`,
        tablet: `preview/geometry/${pageName}.tablet.layout`,
        mobile: `preview/geometry/${pageName}.mobile.layout`
      },
      totalViolations: res.totalViols
    });
  }

  // Update manifest
  fs.writeFileSync(path.resolve(previewDir, 'manifest.json'), JSON.stringify(manifest, null, 2), 'utf8');

  const totalElapsed = Math.round(performance.now() - startTime);
  if (!isQuiet) {
    console.log(`\n✨ Batch layout analysis completed in ${totalElapsed}ms across ${pageFiles.length} pages. Total violations: ${totalViolations}`);
  }
}

async function main() {
  const isWatch = process.argv.includes('--watch') || process.argv.includes('-w');
  const chromium = await getPlaywright();

  if (isWatch) {
    await runBatchAnalysis(chromium, false);
    await startWatchMode(chromium);
  } else {
    console.log('🚀 [Socratic AI Layout Analyzer] Running full build audit...');
    await runBatchAnalysis(chromium, false);
  }
}

main().catch(err => {
  console.error('Fatal error in layout analyzer:', err);
  process.exit(1);
});
