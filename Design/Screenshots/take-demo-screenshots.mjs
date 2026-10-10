import path from 'path';
import fs from 'fs';
import { fileURLToPath, pathToFileURL } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const playwrightPath = path.resolve(__dirname, '../../Tools/socratic-ui-mcp/node_modules/playwright/index.mjs');
const { chromium } = await import(pathToFileURL(playwrightPath).href);

const artifactDir = 'C:\\Users\\owner\\.gemini\\antigravity-ide\\brain\\5453c955-463c-4790-a83f-b9453f01dc71';
if (!fs.existsSync(artifactDir)) {
  fs.mkdirSync(artifactDir, { recursive: true });
}

const targetFiles = [
  {
    name: 'layout',
    title: 'Layout & Grid Showcase',
    path: path.resolve(__dirname, '../../Shared/DesignSystem/Assets/wwwroot/styles-demo.html'),
    stressBtnSelector: '#layoutStressTestBtn'
  },
  {
    name: 'material',
    title: 'Material.Web Showcase',
    path: path.resolve(__dirname, '../../Shared/DesignSystem/Assets/wwwroot/material-web-styles-demo.html'),
    stressBtnSelector: '#stressTestBtn'
  }
];

const viewports = [
  { name: 'mobile', width: 390, height: 844 },
  { name: 'tablet', width: 820, height: 1180 },
  { name: 'desktop', width: 1440, height: 900 }
];

async function captureAll() {
  const browser = await chromium.launch({ headless: true });

  for (const item of targetFiles) {
    const fileUrl = pathToFileURL(item.path).href;
    console.log(`\nCapturing screenshots for ${item.title} (${fileUrl})...`);

    for (const vp of viewports) {
      const context = await browser.newContext({
        viewport: { width: vp.width, height: vp.height },
        deviceScaleFactor: 2
      });
      const page = await context.newPage();

      await page.goto(fileUrl, { waitUntil: 'networkidle' });
      await page.waitForTimeout(600);

      // 1. Baseline viewport screenshot
      const vpShotName = `${item.name}_${vp.name}_viewport.png`;
      const vpShotPath = path.join(artifactDir, vpShotName);
      await page.screenshot({ path: vpShotPath, fullPage: false });
      console.log(`Saved viewport screenshot: ${vpShotName}`);

      // 2. Baseline fullpage screenshot
      const fullShotName = `${item.name}_${vp.name}_full.png`;
      const fullShotPath = path.join(artifactDir, fullShotName);
      await page.screenshot({ path: fullShotPath, fullPage: true });
      console.log(`Saved fullpage screenshot: ${fullShotName}`);

      // 3. Stress test screenshot on desktop
      if (vp.name === 'desktop') {
        try {
          const stressBtn = await page.$(item.stressBtnSelector);
          if (stressBtn) {
            await stressBtn.click();
            await page.waitForTimeout(500);
            const stressShotName = `${item.name}_desktop_stress.png`;
            const stressShotPath = path.join(artifactDir, stressShotName);
            await page.screenshot({ path: stressShotPath, fullPage: false });
            console.log(`Saved stress screenshot: ${stressShotName}`);
          }
        } catch (e) {
          console.warn(`Stress btn error: ${e.message}`);
        }
      }

      await context.close();
    }
  }

  await browser.close();
  console.log('\nAll screenshots captured successfully into artifact directory!');
}

captureAll().catch(err => {
  console.error('Capture failed:', err);
  process.exit(1);
});
