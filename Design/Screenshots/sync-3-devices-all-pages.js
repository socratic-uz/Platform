import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

import { pathToFileURL } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Import Playwright from local Tools module
const playwrightPath = path.resolve(__dirname, '../../Tools/socratic-ui-mcp/node_modules/playwright/index.mjs');
const { chromium } = await import(pathToFileURL(playwrightPath).href);

const screenshotDir = __dirname;
if (!fs.existsSync(screenshotDir)) {
  fs.mkdirSync(screenshotDir, { recursive: true });
}

// Target Base URL: prefer HTTPS (6443) or fallback to HTTP proxy (6080)
const defaultBaseUrl = process.env.BASE_URL || 'https://localhost:6443';

const pages = [
  { path: '/', slug: 'landing', title: 'Universal Landing Page' },
  { path: '/home', slug: 'home', title: 'Director Organization Console' },
  { path: '/chat', slug: 'chat', title: 'AI Assistant & Copilot' },
  { path: '/signin', slug: 'signin', title: 'Authentication & Sign In' },
  { path: '/commerce', slug: 'commerce', title: 'Customer Marketplace & 28 UX Modes' },
  { path: '/kiosk', slug: 'kiosk', title: 'Customer Self-Service Ordering & Menu' },
  { path: '/terminal', slug: 'terminal', title: 'Cashier POS Terminal Workstation' },
  { path: '/wishlist', slug: 'wishlist', title: 'Wishlist & Saved Items' },
  { path: '/orders', slug: 'orders', title: 'Orders Registry & Live Status' },
  { path: '/order-items', slug: 'order-items', title: 'Order Items & Kitchen Display System' },
  { path: '/analytics', slug: 'analytics', title: 'Platform Analytics & Data Registry' },
  { path: '/payment', slug: 'payment', title: 'Checkout & Payment Processing' },
  { path: '/seat-designer', slug: 'seat-designer', title: '3D Seat Designer Studio' },
  { path: '/qr-designer', slug: 'qr-designer', title: 'Dynamic QR Designer Studio' },
  { path: '/map', slug: 'map', title: 'Interactive Map & Delivery Zones' },
  { path: '/detector', slug: 'detector', title: 'Computer Vision & Object Detector' },
  { path: '/components', slug: 'components', title: 'Material.Web Components Showcase' },
  { path: '/docs', slug: 'docs', title: 'System Interactive Documentation' },
  { path: '/about', slug: 'about', title: 'About Socratic Platform' },
  { path: '/help', slug: 'help', title: 'Support Center & Knowledge Base' }
];

const devices = [
  {
    name: 'Desktop',
    slug: 'desktop',
    label: 'Desktop 1440x900',
    viewport: { width: 1440, height: 900 },
    isMobile: false
  },
  {
    name: 'Tablet',
    slug: 'tablet',
    label: 'Tablet 1024x768',
    viewport: { width: 1024, height: 768 },
    isMobile: false
  },
  {
    name: 'Mobile',
    slug: 'mobile',
    label: 'Mobile 390x844',
    viewport: { width: 390, height: 844 },
    isMobile: true
  }
];

// CLI Arguments: --device=desktop|tablet|mobile, --page=landing,home,chat,...
const args = process.argv.slice(2);
const deviceFilter = args.find(a => a.startsWith('--device='))?.split('=')[1] || null;
const pageFilter = args.find(a => a.startsWith('--page='))?.split('=')[1] || null;

const filteredDevices = deviceFilter ? devices.filter(d => d.slug === deviceFilter.toLowerCase()) : devices;
const pageFilters = pageFilter ? pageFilter.toLowerCase().split(',').map(s => s.trim()) : null;
const filteredPages = pageFilters ? pages.filter(p => pageFilters.includes(p.slug)) : pages;

async function run() {
  console.log(`================================================================`);
  console.log(`Socratic Full-Page Visual Screenshot Capture Engine`);
  console.log(`Base URL: ${defaultBaseUrl}`);
  console.log(`Output Directory: ${screenshotDir}`);
  console.log(`Target: ${filteredPages.length} pages x ${filteredDevices.length} devices = ${filteredPages.length * filteredDevices.length} full-page screenshots`);
  console.log(`================================================================\n`);

  const browser = await chromium.launch({
    headless: true,
    args: ['--ignore-certificate-errors', '--no-sandbox']
  });

  const manifest = [];
  let successCount = 0;
  let failCount = 0;

  for (const device of filteredDevices) {
    console.log(`\n>>> Device: ${device.name} (${device.label})`);

    const context = await browser.newContext({
      viewport: device.viewport,
      deviceScaleFactor: 1,
      isMobile: device.isMobile,
      ignoreHTTPSErrors: true
    });

    const page = await context.newPage();

    let pageIndex = 0;
    for (const p of filteredPages) {
      pageIndex++;
      const fullUrl = `${defaultBaseUrl}${p.path}`;
      const fileName = `sync-${device.slug}-${p.slug}.png`;
      const filePath = path.join(screenshotDir, fileName);

      process.stdout.write(`  [${pageIndex}/${filteredPages.length}] ${p.path} (${p.slug})... `);

      try {
        await page.goto(fullUrl, { waitUntil: 'domcontentloaded', timeout: 30000 });
        
        // Wait for Blazor InteractiveAuto hydration and web components
        await page.waitForTimeout(2000);

        // Pre-render scroll to load any lazy images and layout shifts
        await page.evaluate(() => {
          window.scrollTo(0, document.body.scrollHeight);
        });
        await page.waitForTimeout(400);
        await page.evaluate(() => {
          window.scrollTo(0, 0);
        });
        await page.waitForTimeout(200);

        // Capture FULL PAGE screenshot (captures everything beyond viewport)
        await page.screenshot({
          path: filePath,
          fullPage: true
        });

        const stats = fs.statSync(filePath);
        console.log(`✓ OK (${Math.round(stats.size / 1024)} KB)`);

        manifest.push({
          device: device.name,
          badge: device.label,
          width: device.viewport.width,
          height: device.viewport.height,
          slug: p.slug,
          file: fileName,
          title: `[${device.label}] ${p.title}`,
          url: fullUrl,
          fullPage: true,
          capturedAt: new Date().toISOString()
        });

        successCount++;
      } catch (err) {
        console.log(`✗ Error: ${err.message}`);
        failCount++;
      }
    }

    await context.close();
  }

  await browser.close();

  // Save manifest
  const manifestPath = path.join(screenshotDir, 'screenshots-manifest.json');
  fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), 'utf8');
  console.log(`\nUpdated screenshots manifest at: ${manifestPath}`);

  console.log(`\n================================================================`);
  console.log(`Capture complete! Total Success: ${successCount}, Failed: ${failCount}`);
  console.log(`All full-page screenshots stored in: ${screenshotDir}`);
  console.log(`================================================================\n`);
}

run().catch(err => {
  console.error('Fatal execution error:', err);
  process.exit(1);
});
