import path from 'path';
import fs from 'fs';
import { fileURLToPath, pathToFileURL } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const playwrightPath = path.resolve(__dirname, '../../Tools/socratic-ui-mcp/node_modules/playwright/index.mjs');
const { chromium } = await import(pathToFileURL(playwrightPath).href);

const targetFiles = [
  {
    name: 'Layout Styles Showcase',
    path: path.resolve(__dirname, '../../Shared/DesignSystem/Assets/wwwroot/styles-demo.html'),
    stressBtnSelector: '#layoutStressTestBtn',
    auditBtnSelector: '#runAuditBtn',
    auditResultSelector: '#auditResults'
  },
  {
    name: 'Material.Web Styles Showcase',
    path: path.resolve(__dirname, '../../Shared/DesignSystem/Assets/wwwroot/material-web-styles-demo.html'),
    stressBtnSelector: '#stressTestBtn',
    auditBtnSelector: '#runAuditBtn',
    auditResultSelector: '#auditResults'
  }
];

const viewports = [
  { name: 'Mobile (390px)', width: 390, height: 844 },
  { name: 'Tablet (820px)', width: 820, height: 1180 },
  { name: 'Desktop (1440px)', width: 1440, height: 900 }
];

async function runAudit() {
  const browser = await chromium.launch({ headless: true });
  const report = [];

  for (const file of targetFiles) {
    const fileUrl = pathToFileURL(file.path).href;
    console.log(`\n======================================================`);
    console.log(`Auditing: ${file.name}`);
    console.log(`File: ${fileUrl}`);
    console.log(`======================================================`);

    const fileReport = {
      name: file.name,
      file: file.path,
      viewports: []
    };

    for (const vp of viewports) {
      console.log(`\n--- Viewport: ${vp.name} (${vp.width}x${vp.height}) ---`);
      const context = await browser.newContext({
        viewport: { width: vp.width, height: vp.height },
        deviceScaleFactor: 2
      });
      const page = await context.newPage();

      // Collect console errors
      const consoleErrors = [];
      page.on('console', msg => {
        if (msg.type() === 'error') consoleErrors.push(msg.text());
      });

      await page.goto(fileUrl, { waitUntil: 'networkidle' });
      await page.waitForTimeout(500);

      // Baseline checks
      const baseline = await page.evaluate(() => {
        const docEl = document.documentElement;
        const body = document.body;
        const hasHScroll = docEl.scrollWidth > docEl.clientWidth || body.scrollWidth > body.clientWidth;

        // Find overflowing elements
        const overflowingElements = [];
        const allElements = document.querySelectorAll('*');
        for (const el of allElements) {
          if (el.scrollWidth > el.clientWidth + 1) {
            // Check if overflow is intentionally scrollable
            const style = window.getComputedStyle(el);
            const isScrollable = style.overflowX === 'auto' || style.overflowX === 'scroll';
            if (!isScrollable && el.clientWidth > 0) {
              overflowingElements.push({
                tag: el.tagName.toLowerCase(),
                className: el.className || '',
                id: el.id || '',
                clientWidth: el.clientWidth,
                scrollWidth: el.scrollWidth,
                diff: el.scrollWidth - el.clientWidth,
                textSnippet: (el.innerText || '').slice(0, 50).trim()
              });
            }
          }
        }

        // Touch target check
        const smallTouchTargets = [];
        const interactiveSelectors = 'button, a, input, select, textarea, [role="button"], md-filled-button, md-outlined-button, md-text-button, md-icon-button';
        const interactives = document.querySelectorAll(interactiveSelectors);
        for (const btn of interactives) {
          const rect = btn.getBoundingClientRect();
          // Filter visible elements
          if (rect.width > 0 && rect.height > 0) {
            const style = window.getComputedStyle(btn);
            if (style.visibility !== 'hidden' && style.display !== 'none' && style.opacity !== '0') {
              if (rect.width < 44 || rect.height < 44) {
                smallTouchTargets.push({
                  tag: btn.tagName.toLowerCase(),
                  className: btn.className || '',
                  width: Math.round(rect.width),
                  height: Math.round(rect.height),
                  textSnippet: (btn.innerText || btn.getAttribute('aria-label') || btn.title || '').slice(0, 30).trim()
                });
              }
            }
          }
        }

        return {
          hasHScroll,
          docScrollWidth: docEl.scrollWidth,
          docClientWidth: docEl.clientWidth,
          overflowingElements: overflowingElements.slice(0, 15),
          totalOverflowing: overflowingElements.length,
          smallTouchTargets: smallTouchTargets.slice(0, 15),
          totalSmallTouchTargets: smallTouchTargets.length
        };
      });

      console.log(`[Baseline] Page H-Scroll: ${baseline.hasHScroll ? '❌ YES (' + baseline.docScrollWidth + 'px > ' + baseline.docClientWidth + 'px)' : '✅ NO'}`);
      console.log(`[Baseline] Overflowing elements: ${baseline.totalOverflowing}`);
      if (baseline.totalOverflowing > 0) {
        baseline.overflowingElements.slice(0, 5).forEach(o => {
          console.log(`   - <${o.tag} class="${o.className}"> diff: +${o.diff}px (client: ${o.clientWidth}, scroll: ${o.scrollWidth}) text: "${o.textSnippet}"`);
        });
      }
      console.log(`[Baseline] Small Touch Targets (<44px): ${baseline.totalSmallTouchTargets}`);
      if (baseline.totalSmallTouchTargets > 0) {
        baseline.smallTouchTargets.slice(0, 5).forEach(t => {
          console.log(`   - <${t.tag} class="${t.className}"> ${t.width}x${t.height}px text: "${t.textSnippet}"`);
        });
      }

      // Run In-Page Audit Button
      let inPageAuditHtml = '';
      try {
        const auditBtn = await page.$(file.auditBtnSelector);
        if (auditBtn) {
          await auditBtn.click();
          await page.waitForTimeout(300);
          inPageAuditHtml = await page.$eval(file.auditResultSelector, el => el.innerText);
          console.log(`[In-Page Audit summary]:\n${inPageAuditHtml.split('\n').slice(0, 8).join('\n')}`);
        }
      } catch (e) {
        console.log(`[In-Page Audit] Note: ${e.message}`);
      }

      // Stress Test
      let stressResult = null;
      try {
        const stressBtn = await page.$(file.stressBtnSelector);
        if (stressBtn) {
          await stressBtn.click();
          await page.waitForTimeout(500);

          stressResult = await page.evaluate(() => {
            const docEl = document.documentElement;
            const body = document.body;
            const hasHScroll = docEl.scrollWidth > docEl.clientWidth || body.scrollWidth > body.clientWidth;

            const blowouts = [];
            const allElements = document.querySelectorAll('*');
            for (const el of allElements) {
              if (el.scrollWidth > el.clientWidth + 2) {
                const style = window.getComputedStyle(el);
                const isScrollable = style.overflowX === 'auto' || style.overflowX === 'scroll';
                if (!isScrollable && el.clientWidth > 0) {
                  blowouts.push({
                    tag: el.tagName.toLowerCase(),
                    className: el.className || '',
                    diff: el.scrollWidth - el.clientWidth,
                    clientWidth: el.clientWidth,
                    scrollWidth: el.scrollWidth,
                    textSnippet: (el.innerText || '').slice(0, 60).trim()
                  });
                }
              }
            }

            return {
              hasHScroll,
              docScrollWidth: docEl.scrollWidth,
              docClientWidth: docEl.clientWidth,
              blowouts: blowouts.slice(0, 15),
              totalBlowouts: blowouts.length
            };
          });

          console.log(`[Stress Test] Page H-Scroll after stress text: ${stressResult.hasHScroll ? '❌ YES (' + stressResult.docScrollWidth + 'px > ' + stressResult.docClientWidth + 'px)' : '✅ NO'}`);
          console.log(`[Stress Test] Total blowouts: ${stressResult.totalBlowouts}`);
          if (stressResult.totalBlowouts > 0) {
            stressResult.blowouts.slice(0, 6).forEach(b => {
              console.log(`   - <${b.tag} class="${b.className}"> diff: +${b.diff}px text: "${b.textSnippet.slice(0, 40)}"`);
            });
          }
        }
      } catch (e) {
        console.log(`[Stress Test Error]: ${e.message}`);
      }

      // Save screenshot for mobile viewport
      const screenshotName = `${path.basename(file.path, '.html')}_${vp.name.toLowerCase().replace(/[^a-z0-9]/g, '_')}.png`;
      const screenshotPath = path.resolve(__dirname, screenshotName);
      await page.screenshot({ path: screenshotPath, fullPage: false });

      fileReport.viewports.push({
        viewport: vp.name,
        baseline,
        stressResult,
        consoleErrors
      });

      await context.close();
    }

    report.push(fileReport);
  }

  await browser.close();

  const reportOutPath = path.resolve(__dirname, 'audit_styles_results.json');
  fs.writeFileSync(reportOutPath, JSON.stringify(report, null, 2), 'utf-8');
  console.log(`\nAuditing completed! Results written to ${reportOutPath}`);
}

runAudit().catch(err => {
  console.error('Audit failed:', err);
  process.exit(1);
});
