import { execSync } from 'child_process';
import path from 'path';
import fs from 'fs';

const screenshotDir = 'c:\\Users\\owner\\source\\repos\\Socratic\\src\\Frontend\\Platform\\Design\\Screenshots';
process.env.STITCH_API_KEY = process.env.STITCH_API_KEY || '';

const targetProjectId = '12274857616076215299';

const pages = [
  { slug: 'landing', title: 'Universal Landing Page' },
  { slug: 'about', title: 'About Socratic Platform' },
  { slug: 'docs', title: 'System Interactive Documentation' },
  { slug: 'help', title: 'Support Center & FAQ' },
  { slug: 'example', title: 'Interactive Scenario Examples' },
  { slug: 'data', title: 'System Data & Metrics Viewer' },
  { slug: 'player', title: 'Media Player & Presentations' },
  { slug: 'qr', title: 'Dynamic QR Scanner & Viewer' },
  { slug: '3d', title: '3D WebGL Interactive Scene' },
  { slug: 'signin', title: 'Authentication & Sign In' },
  { slug: 'home', title: 'Director Organization Console' },
  { slug: 'commerce', title: 'Customer Marketplace & 28 UX Modes' },
  { slug: 'terminal', title: 'Cashier POS Terminal Workstation' },
  { slug: 'wishlist', title: 'Wishlist & Saved Items' },
  { slug: 'kiosk', title: 'Customer Self-Service Ordering & Menu' },
  { slug: 'orders', title: 'Orders Registry & Live Status' },
  { slug: 'order-items', title: 'Order Items & Kitchen Display System' },
  { slug: 'payment', title: 'Checkout & Payment Processing' },
  { slug: 'seat-designer', title: '3D Seat Designer Studio' },
  { slug: 'qr-designer', title: 'Dynamic QR Designer Studio' },
  { slug: 'map', title: 'Interactive Map & Delivery Zones' },
  { slug: 'chat', title: 'AI Assistant & Copilot' },
  { slug: 'detector', title: 'Computer Vision & Object Detector' },
  { slug: 'components', title: 'Material.Web Components Library Showcase' }
];

const devices = [
  { slug: 'mobile', prefix: '[Mobile 390x844]' },
  { slug: 'tablet', prefix: '[Tablet 1024x768]' },
  { slug: 'desktop', prefix: '[Desktop 1440x900]' }
];

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function uploadToStitchWithRetry(filePath, title, maxRetries = 3) {
  const cmd = `npx -y @_davideast/stitch-mcp upload-image --project ${targetProjectId} --file "${filePath}" --title "${title}"`;
  
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      const output = execSync(cmd, { env: process.env, encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] });
      if (output.includes('screenId') || output.includes('Successfully uploaded')) {
        return true;
      }
    } catch (e) {
      const out = (e.stdout || '') + (e.stderr || '');
      if (out.includes('screenId') || out.includes('Successfully uploaded')) {
        return true;
      }
      if (attempt === maxRetries) {
        console.error(`\nFailed to upload ${title} after ${maxRetries} attempts:`, out);
        return false;
      }
      execSync(`node -e "setTimeout(() => {}, ${attempt * 1500})"`);
    }
  }
  return false;
}

async function uploadAll() {
  console.log(`Starting clean upload of all 72 screens to Master Project: ${targetProjectId}...\n`);
  
  let count = 0;
  const total = devices.length * pages.length;

  for (const device of devices) {
    console.log(`\n================================================================`);
    console.log(`Uploading ${device.prefix} screens (${pages.length} screens)...`);
    console.log(`================================================================`);

    for (const p of pages) {
      count++;
      const fileName = `sync-${device.slug}-${p.slug}.png`;
      const filePath = path.join(screenshotDir, fileName);
      const title = `${device.prefix} ${p.title}`;

      if (!fs.existsSync(filePath)) {
        console.warn(`[${count}/${total}] File missing: ${filePath}`);
        continue;
      }

      const size = fs.statSync(filePath).size;
      process.stdout.write(`[${count}/${total}] Uploading "${title}" (${size} bytes)... `);
      const success = uploadToStitchWithRetry(filePath, title);
      if (success) {
        console.log(`✓ Uploaded`);
      } else {
        console.log(`✗ Failed`);
      }
      await sleep(350);
    }
  }

  console.log(`\n================================================================`);
  console.log(`All ${count} screens successfully uploaded to Master Project ${targetProjectId}!`);
  console.log(`================================================================\n`);
}

uploadAll().catch(console.error);
