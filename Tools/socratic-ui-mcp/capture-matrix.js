import { chromium } from 'playwright';
import path from 'path';
import fs from 'fs';

const screenshotDir = 'c:\\Users\\owner\\source\\repos\\Socratic\\src\\Frontend\\Platform\\Design\\Screenshots';
if (!fs.existsSync(screenshotDir)) {
  fs.mkdirSync(screenshotDir, { recursive: true });
}

const baseUrl = process.env.UI_BASE_URL ?? 'http://localhost:6080';

// Canonical 24 Universal Socratic Pages
const pages = [
  { path: '/', slug: 'landing', title: 'Universal Landing Page' },
  { path: '/about', slug: 'about', title: 'About Socratic Platform' },
  { path: '/docs', slug: 'docs', title: 'System Interactive Documentation' },
  { path: '/help', slug: 'help', title: 'Support Center & FAQ' },
  { path: '/example', slug: 'example', title: 'Interactive Scenario Examples' },
  { path: '/data', slug: 'data', title: 'System Data & Metrics Viewer' },
  { path: '/player', slug: 'player', title: 'Media Player & Presentations' },
  { path: '/qr', slug: 'qr', title: 'Dynamic QR Scanner & Viewer' },
  { path: '/3d', slug: '3d', title: '3D WebGL Interactive Scene' },
  { path: '/signin', slug: 'signin', title: 'Authentication & Sign In' },
  { path: '/home', slug: 'home', title: 'Director Organization Console' },
  { path: '/commerce', slug: 'commerce', title: 'Customer Marketplace & 28 UX Modes' },
  { path: '/terminal', slug: 'terminal', title: 'Cashier POS Terminal Workstation' },
  { path: '/wishlist', slug: 'wishlist', title: 'Wishlist & Saved Items' },
  { path: '/kiosk', slug: 'kiosk', title: 'Customer Self-Service Ordering & Menu' },
  { path: '/orders', slug: 'orders', title: 'Orders Registry & Live Status' },
  { path: '/order-items', slug: 'order-items', title: 'Order Items & Kitchen Display System' },
  { path: '/payment', slug: 'payment', title: 'Checkout & Payment Processing' },
  { path: '/seat-designer', slug: 'seat-designer', title: '3D Seat Designer Studio' },
  { path: '/qr-designer', slug: 'qr-designer', title: 'Dynamic QR Designer Studio' },
  { path: '/map', slug: 'map', title: 'Interactive Map & Delivery Zones' },
  { path: '/chat', slug: 'chat', title: 'AI Assistant & Copilot' },
  { path: '/detector', slug: 'detector', title: 'Computer Vision & Object Detector' },
  { path: '/components', slug: 'components', title: 'Material.Web Components Library Showcase' }
];

// Device Form Factors Matrix
const devices = [
  {
    name: 'Mobile',
    slug: 'mobile',
    badge: 'Mobile 390x844',
    viewport: { width: 390, height: 844 },
    isMobile: true,
    hasTouch: true
  },
  {
    name: 'Tablet',
    slug: 'tablet',
    badge: 'Tablet 1024x768',
    viewport: { width: 1024, height: 768 },
    isMobile: false,
    hasTouch: true
  },
  {
    name: 'Desktop',
    slug: 'desktop',
    badge: 'Desktop 1440x900',
    viewport: { width: 1440, height: 900 },
    isMobile: false,
    hasTouch: false
  }
];

async function captureMatrix() {
  console.log('Starting Socratic Multi-Device UI Capture Matrix...');
  console.log(`Base URL: ${baseUrl}`);
  console.log(`Destination Directory: ${screenshotDir}\n`);

  const browser = await chromium.launch({
    headless: true,
    channel: 'chrome',
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  const manifest = [];
  let count = 0;
  const total = devices.length * pages.length;

  for (const device of devices) {
    console.log(`\n================================================================`);
    console.log(`Capturing for Device: ${device.badge} (${pages.length} screens)`);
    console.log(`================================================================`);

    const context = await browser.newContext({
      viewport: device.viewport,
      deviceScaleFactor: 1,
      isMobile: device.isMobile,
      hasTouch: device.hasTouch
    });

    const page = await context.newPage();

    for (const p of pages) {
      count++;
      const fullUrl = `${baseUrl}${p.path}`;
      const fileName = `sync-${device.slug}-${p.slug}.png`;
      const outPath = path.join(screenshotDir, fileName);
      const title = `[${device.badge}] ${p.title}`;

      process.stdout.write(`[${count}/${total}] Navigating to ${p.path} (${device.name})... `);

      try {
        await page.goto(fullUrl, { waitUntil: 'domcontentloaded', timeout: 30000 });
        // Allow Blazor InteractiveAuto WASM hydration and animations to settle
        await page.waitForTimeout(3000);

        await page.screenshot({ path: outPath, fullPage: true });
        const stat = fs.statSync(outPath);
        console.log(`✓ (${Math.round(stat.size / 1024)} KB) -> ${fileName}`);

        manifest.push({
          device: device.name,
          slug: device.slug,
          badge: device.badge,
          width: device.viewport.width,
          height: device.viewport.height,
          fileName,
          title,
          url: fullUrl,
          path: p.path,
          sizeBytes: stat.size,
          timestamp: new Date().toISOString()
        });
      } catch (err) {
        console.log(`✗ Error: ${err.message}`);
      }
    }

    await context.close();
  }

  await browser.close();

  const manifestPath = path.join(screenshotDir, 'screenshots-manifest.json');
  fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), 'utf8');

  console.log(`\n================================================================`);
  console.log(`Multi-Device Capture complete!`);
  console.log(`Saved: ${manifest.length} of ${total} screenshots.`);
  console.log(`Manifest: ${manifestPath}`);
  console.log(`================================================================\n`);
}

captureMatrix().catch(console.error);
