/**
 * bundle-assets.mjs — Release / Native Single-File CSS & JS Bundler
 *
 * Bundles modular CSS (excluding informative tokens/_md-*.css) into a single
 * @layer-preserving styles.css and bundles scripts.js into a single ES module
 * via Rollup without modifying source files in wwwroot/.
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import postcss from 'postcss';
import { rollup } from 'rollup';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');
const cssRoot = path.join(assetsRoot, 'wwwroot', 'css');
const jsEntry = path.join(assetsRoot, 'wwwroot', 'js', 'scripts.js');

function parseOutDir() {
  const idx = process.argv.indexOf('--out');
  if (idx !== -1 && process.argv[idx + 1]) {
    return path.resolve(assetsRoot, process.argv[idx + 1]);
  }
  return path.join(assetsRoot, 'obj', 'Release', 'release-bundle');
}

function findCssFiles(dir) {
  const results = [];
  if (!fs.existsSync(dir)) return results;

  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...findCssFiles(fullPath));
    } else if (
      entry.isFile() &&
      entry.name.endsWith('.css') &&
      !entry.name.endsWith('.min.css') &&
      entry.name !== 'styles.css'
    ) {
      results.push(fullPath);
    }
  }
  return results;
}

function sortLayerFiles(files, isFoundations = false) {
  return [...files].sort((a, b) => {
    if (isFoundations) {
      const aIsReset = path.basename(a) === 'reset.css';
      const bIsReset = path.basename(b) === 'reset.css';
      if (aIsReset && !bIsReset) return -1;
      if (!aIsReset && bIsReset) return 1;
    }
    return a.localeCompare(b, undefined, { numeric: true, sensitivity: 'base' });
  });
}

function rewriteRelativeUrls(cssText, sourceFile) {
  const sourceDir = path.dirname(sourceFile);
  return cssText.replace(/url\(\s*(['"]?)([^'")]+)\1\s*\)/g, (match, quote, rawUrl) => {
    const trimmed = rawUrl.trim();
    if (
      !trimmed ||
      trimmed.startsWith('data:') ||
      trimmed.startsWith('http://') ||
      trimmed.startsWith('https://') ||
      trimmed.startsWith('/') ||
      trimmed.startsWith('#')
    ) {
      return match;
    }
    const resolved = path.resolve(sourceDir, trimmed);
    const relFromCssRoot = path.relative(cssRoot, resolved).replace(/\\/g, '/');
    const q = quote || "'";
    return `url(${q}${relFromCssRoot}${q})`;
  });
}

function stripCommentsAndMinifyCss(cssText) {
  const root = postcss.parse(cssText);
  root.walkComments((comment) => comment.remove());
  return root.toResult().css
    .replace(/\r?\n\s*/g, '')
    .replace(/\s*([{}:;,])\s*/g, '$1')
    .replace(/;}/g, '}')
    .trim();
}

async function bundleCss(outCssPath) {
  const allFiles = findCssFiles(cssRoot);
  const layers = {
    fonts: [],
    tokens: [],
    foundations: [],
    components: [],
    widgets: [],
    pages: [],
    layouts: [],
    other: []
  };

  let skippedMdCount = 0;
  for (const fullPath of allFiles) {
    const relPath = path.relative(cssRoot, fullPath).replace(/\\/g, '/');
    const topFolder = relPath.split('/')[0].toLowerCase();
    const baseName = path.basename(fullPath);

    if (topFolder === 'tokens' && baseName.startsWith('_md-')) {
      skippedMdCount++;
      continue;
    }

    if (layers[topFolder]) {
      layers[topFolder].push(fullPath);
    } else {
      layers.other.push(fullPath);
    }
  }

  const orderedGroups = [
    { name: null, files: sortLayerFiles(layers.fonts) },
    { name: 'tokens', files: sortLayerFiles(layers.tokens) },
    { name: 'foundations', files: sortLayerFiles(layers.foundations, true) },
    { name: 'components', files: sortLayerFiles(layers.components) },
    { name: 'widgets', files: sortLayerFiles(layers.widgets) },
    { name: 'pages', files: sortLayerFiles(layers.pages) },
    { name: 'layouts', files: sortLayerFiles(layers.layouts) },
    { name: null, files: sortLayerFiles(layers.other) }
  ];

  const chunks = ['@layer tokens,foundations,components,widgets,pages,layouts;'];
  let includedCount = 0;

  for (const group of orderedGroups) {
    if (group.files.length === 0) continue;

    const combinedRaw = group.files
      .map((filePath) => rewriteRelativeUrls(fs.readFileSync(filePath, 'utf8'), filePath))
      .join('\n');
    const minified = stripCommentsAndMinifyCss(combinedRaw);
    if (!minified) continue;

    includedCount += group.files.length;
    if (group.name) {
      chunks.push(`@layer ${group.name}{${minified}}`);
    } else {
      chunks.push(minified);
    }
  }

  fs.mkdirSync(path.dirname(outCssPath), { recursive: true });
  fs.writeFileSync(outCssPath, chunks.join('\n'), 'utf8');
  const sizeKb = (Buffer.byteLength(chunks.join('\n'), 'utf8') / 1024).toFixed(1);
  console.log(`✅ [release-bundle] CSS: ${includedCount} files -> ${outCssPath} (${sizeKb} KB, skipped ${skippedMdCount} _md-*.css)`);
}

async function bundleJs(outJsPath) {
  fs.mkdirSync(path.dirname(outJsPath), { recursive: true });
  const bundle = await rollup({
    input: jsEntry,
    plugins: [resolve(), commonjs()],
    onwarn(warning, warn) {
      if (warning.code === 'CIRCULAR_DEPENDENCY' || warning.code === 'THIS_IS_UNDEFINED') return;
      warn(warning);
    }
  });

  try {
    await bundle.write({
      file: outJsPath,
      format: 'es',
      compact: true
    });
  } finally {
    await bundle.close();
  }

  const sizeKb = (fs.statSync(outJsPath).size / 1024).toFixed(1);
  console.log(`✅ [release-bundle] JS: scripts.js -> ${outJsPath} (${sizeKb} KB)`);
}

async function main() {
  const outDir = parseOutDir();
  await bundleCss(path.join(outDir, 'css', 'styles.css'));
  await bundleJs(path.join(outDir, 'js', 'scripts.js'));
}

main().catch((err) => {
  console.error('❌ [release-bundle] Failed:', err);
  process.exit(1);
});
