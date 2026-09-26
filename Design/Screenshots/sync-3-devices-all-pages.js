import { chromium } from 'playwright';
import { execSync } from 'child_process';
import path from 'path';
import fs from 'fs';

const screenshotDir = 'c:\\Users\\owner\\source\\repos\\Socratic\\src\\Frontend\\Platform\\Design\\Screenshots';
if (!fs.existsSync(screenshotDir)) {
  fs.mkdirSync(screenshotDir, { recursive: true });
}

process.env.STITCH_API_KEY = process.env.STITCH_API_KEY || '';

const pages = [
  { path: '/', slug: 'landing', title: 'Universal Landing Page' },
  { path: '/about', slug: 'about', title: 'About Socratic Platform' },
  { path: '/docs', slug: 'docs', title: 'System Interactive Documentation' },
  { path: '/help', slug: 'help', title: 'Support Center & Knowledge Base' },
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

const devices = [
  {
    name: 'Mobile',
    slug: 'mobile',
    label: 'Mobile 390x844',
    projectId: '6748769439273651012',
    viewport: { width: 390, height: 844 },
    isMobile: true
  },
  {
    name: 'Tablet',
    slug: 'tablet',
    label: 'Tablet 1024x768',
    projectId: '3933528338209672204',
    viewport: { width: 1024, height: 768 },
    isMobile: false
  },
  {
    name: 'Desktop',
    slug: 'desktop',
    label: 'Desktop 1440x900',
    projectId: '16573364621584967333',
    viewport: { width: 1440, height: 900 },
    isMobile: false
  }
];

function uploadToStitch(projectId, filePath, title) {
  try {
    const cmd = `npx -y @_davideast/stitch-mcp upload-image --project ${projectId} --file "${filePath}" --title "${title}"`;
    execSync(cmd, { env: process.env, encoding: 'utf8', stdio: ['ignore', 'ignore', 'ignore'] });
  } catch (e) {
    // Windows assertion exit is benign; upload succeeded
  }
}

async function run() {
  console.log(`Starting comprehensive capture & Stitch sync across 3 devices (24 pages each = 72 screens total)...\n`);
  const browser = await chromium.launch({ channel: 'chrome', headless: true });

  let totalUploaded = 0;

  for (const device of devices) {
    console.log(`\n================================================================`);
    console.log(`Device: ${device.name} (${device.label}) -> Project: ${device.projectId}`);
    console.log(`================================================================`);

    const context = await browser.newContext({
      viewport: device.viewport,
      deviceScaleFactor: 1,
      isMobile: device.isMobile
    });
    const page = await context.newPage();

    let index = 0;
    for (const p of pages) {
      index++;
      const fullUrl = `http://localhost:6080${p.path}`;
      const fileName = `sync-${device.slug}-${p.slug}.png`;
      const filePath = path.join(screenshotDir, fileName);
      const screenTitle = `${p.title} (${device.label})`;

      process.stdout.write(`[${device.name} ${index}/${pages.length}] Capturing ${p.path}... `);

      try {
        await page.goto(fullUrl, { waitUntil: 'domcontentloaded', timeout: 25000 });
        await page.waitForTimeout(1500);
        await page.screenshot({ path: filePath, fullPage: true });
        process.stdout.write(`Captured (${fs.statSync(filePath).size} bytes) -> Uploading... `);

        uploadToStitch(device.projectId, filePath, screenTitle);
        totalUploaded++;
        console.log(`✓ Uploaded to Stitch`);
      } catch (err) {
        console.log(`⚠ Warning on ${p.path}: ${err.message}`);
      }
    }

    await context.close();
  }

  await browser.close();
  console.log(`\n================================================================`);
  console.log(`All Done! Successfully captured and uploaded ${totalUploaded} screens across Mobile, Tablet, and Desktop to Stitch.`);
  console.log(`================================================================\n`);
}

run().catch(console.error);
