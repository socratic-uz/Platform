import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import zlib from 'node:zlib';
import * as lightningcss from 'lightningcss';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');

const entryCssPath = path.join(assetsRoot, 'wwwroot', 'css', 'styles.css');
const stylesCssDir = path.dirname(entryCssPath);

const primaryBundlePath = path.join(stylesCssDir, 'styles.bundle.min.css');
const legacySocraticBundlePath = path.join(stylesCssDir, 'socratic.bundle.min.css');

function saveCompressed(filePath, content) {
  fs.writeFileSync(filePath, content);
  return { raw: content.length };
}

function bundleCss() {
  console.log(`\n⚡ [LightningCSS Bundler] Compiling master stylesheet: ${entryCssPath}`);
  const startTime = performance.now();

  if (!fs.existsSync(entryCssPath)) {
    console.error(`❌ Entry CSS file not found: ${entryCssPath}`);
    process.exit(1);
  }

  try {
    const result = lightningcss.bundle({
      filename: entryCssPath,
      minify: true,
      sourceMap: true,
      targets: {
        chrome: 110 << 16,
        firefox: 110 << 16,
        safari: 16 << 16,
        edge: 110 << 16,
      },
    });

    // 1. Primary W3C bundle: styles.bundle.min.css
    const sizes = saveCompressed(primaryBundlePath, result.code);

    if (result.map) {
      fs.writeFileSync(`${primaryBundlePath}.map`, result.map);
    }

    // 2. Compatibility mirror: socratic.bundle.min.css
    saveCompressed(legacySocraticBundlePath, result.code);
    if (result.map) {
      fs.writeFileSync(`${legacySocraticBundlePath}.map`, result.map);
    }

    const duration = (performance.now() - startTime).toFixed(1);
    const rawKb = (sizes.raw / 1024).toFixed(1);

    console.log(`✅ Production CSS bundle created in ${duration}ms!`);
    console.log(`   - Raw minified: ${rawKb} KB`);

    // 3. Deconsolidated components bundle: components/components.bundle.min.css
    const componentsCssPath = path.join(stylesCssDir, 'components', 'components.css');
    const componentsBundlePath = path.join(stylesCssDir, 'components', 'components.bundle.min.css');
    if (fs.existsSync(componentsCssPath)) {
      const compResult = lightningcss.bundle({
        filename: componentsCssPath,
        minify: true,
        sourceMap: true,
        targets: {
          chrome: 110 << 16,
          firefox: 110 << 16,
          safari: 16 << 16,
          edge: 110 << 16,
        },
      });
      const compSizes = saveCompressed(componentsBundlePath, compResult.code);
      if (compResult.map) {
        fs.writeFileSync(`${componentsBundlePath}.map`, compResult.map);
      }
      const compKb = (compSizes.raw / 1024).toFixed(1);
      console.log(`✅ Production Components CSS bundle created: ${compKb} KB`);
    }
  } catch (err) {
    console.error(`❌ LightningCSS compilation failed:`, err);
    process.exit(1);
  }
}

bundleCss();
