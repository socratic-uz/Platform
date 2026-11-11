/**
 * extract-material-tokens.mjs — Material Web Tokens Extractor (Per-file Mirror)
 *
 * Extracts all official tokens from @material/web and outputs them into
 * individual matching files in wwwroot/css/tokens/ matching the package structure:
 *   - _md-comp-*.css (for each component token manifest)
 *   - _md-sys-*.css (for system token manifests)
 *   - _md-ref-*.css (for reference palettes & typefaces)
 *
 * Run: npm run extract:tokens
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const assetsRoot = path.resolve(__dirname, '..');
const pkgRoot = path.join(assetsRoot, 'node_modules', '@material', 'web');
const tokensDir = path.join(assetsRoot, 'wwwroot', 'css', 'tokens');
const pkgTokensDir = path.join(pkgRoot, 'tokens');
const vDir = path.join(pkgTokensDir, 'versions', 'v0_192');

if (!fs.existsSync(pkgRoot)) {
  console.error(`❌ [extract-tokens] @material/web not found at ${pkgRoot}`);
  process.exit(1);
}

let pkgVersion = 'unknown';
try {
  const pkgJson = JSON.parse(fs.readFileSync(path.join(pkgRoot, 'package.json'), 'utf8'));
  pkgVersion = pkgJson.version || 'unknown';
} catch {}

console.log(`\n🔍 [extract-tokens] Extracting individual token files from @material/web v${pkgVersion}...`);

// 1. Collect all var(--md-...) from all *-styles.css
function getStyleFiles(dir) {
  const results = [];
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  for (const entry of entries) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...getStyleFiles(full));
    } else if (entry.isFile() && entry.name.endsWith('-styles.css')) {
      results.push(full);
    }
  }
  return results;
}

const allStyles = getStyleFiles(pkgRoot);
const allTokensMap = new Map();

for (const filePath of allStyles) {
  const content = fs.readFileSync(filePath, 'utf8');
  let idx = 0;
  while ((idx = content.indexOf('var(--md-', idx)) !== -1) {
    const start = idx + 4;
    let depth = 1;
    let end = start;
    while (end < content.length && depth > 0) {
      if (content[end] === '(') depth++;
      else if (content[end] === ')') depth--;
      end++;
    }
    const varExpr = content.substring(start, end - 1);
    idx = end;

    const commaIdx = varExpr.indexOf(',');
    let token = commaIdx !== -1 ? varExpr.substring(0, commaIdx).trim() : varExpr.trim();
    let fallback = commaIdx !== -1 ? varExpr.substring(commaIdx + 1).trim() : '';

    if (!token.startsWith('--md-sys-')) {
      if (!allTokensMap.has(token) || (!allTokensMap.get(token) && fallback)) {
        allTokensMap.set(token, fallback);
      }
    }
  }
}

// 2. Normalize values for Socratic M3 and stylelint compliance
function normalizeFallback(val) {
  if (!val) return 'initial';
  // Replace internal component variables with public token references or fallback
  val = val.replace(/var\(--_container-height\)/g, '32px');
  val = val.replace(/--_([a-z0-9-]+)/g, '--md-$1');
  // Strip redundant hex from var(--md-sys-color-*, #hex) -> var(--md-sys-color-*)
  val = val.replace(/var\((--md-sys-color-[a-z0-9-]+),\s*#[a-fA-F0-9]+\)/g, 'var($1)');
  // Strip redundant font fallback inside var(--md-ref-typeface-*, Roboto) -> var(--md-ref-typeface-*)
  val = val.replace(/var\((--md-ref-typeface-[a-z0-9-]+),\s*[a-zA-Z0-9\s,-]+\)/g, 'var($1)');
  // Limit decimal precision to 4 digits for stylelint number-max-precision
  val = val.replace(/(-?\d+\.\d{5,})/g, m => parseFloat(Number(m).toFixed(4)).toString());
  // Replace standalone hex with rgb(...)
  val = val.replace(/#000000|#000\b/g, 'rgb(0 0 0)');
  val = val.replace(/#ffffff|#fff\b/g, 'rgb(255 255 255)');
  val = val.replace(/#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})/g, (_, r, g, b) => {
    return `rgb(${parseInt(r, 16)} ${parseInt(g, 16)} ${parseInt(b, 16)})`;
  });
  // Replace rgba(r, g, b, a) -> rgb(r g b / a)
  val = val.replace(/rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([0-9.]+)\s*\)/g, 'rgb($1 $2 $3 / $4)');
  return val;
}

// 3. Map tokens to each component file
const compFiles = fs.readdirSync(pkgTokensDir)
  .filter(f => f.startsWith('_md-comp-') && f.endsWith('.scss') && !f.includes('test'));

const compConfigs = [];
for (const f of compFiles) {
  const compName = f.replace(/^_md-comp-/, '').replace(/\.scss$/, '');
  const outName = `_md-comp-${compName}.css`;
  compConfigs.push({
    sourceFile: f,
    compName,
    outName,
    prefix: `--md-${compName}-`,
    tokens: []
  });
}

// Sort longest prefix first to match specific prefixes before general ones
const sortedConfigs = [...compConfigs].sort((a, b) => b.prefix.length - a.prefix.length);

for (const [token, fallback] of allTokensMap) {
  for (const cfg of sortedConfigs) {
    if (token.startsWith(cfg.prefix)) {
      cfg.tokens.push({ token, value: normalizeFallback(fallback) });
      break;
    }
  }
}

let generatedCompCount = 0;
for (const cfg of compConfigs) {
  if (cfg.tokens.length === 0) continue;

  cfg.tokens.sort((a, b) => a.token.localeCompare(b.token));

  let css = `/* ==========================================================================
   Material Web Component Tokens: ${cfg.outName}
   Component: ${cfg.compName} (${cfg.tokens.length} tokens)
   Package: @material/web v${pkgVersion}
   Auto-generated by scripts/extract-material-tokens.mjs. DO NOT EDIT DIRECTLY.
   ========================================================================== */

:root {
`;
  for (const t of cfg.tokens) {
    css += `    ${t.token}: ${t.value};\n`;
  }
  css += `}\n`;

  const outPath = path.join(tokensDir, cfg.outName);
  fs.writeFileSync(outPath, css, 'utf8');
  generatedCompCount++;
}

console.log(`✅ Generated ${generatedCompCount} component token files in wwwroot/css/tokens/`);

// 4. Extract standard system token files (_md-sys-shape, motion, elevation, state, typescale)
const sysStandardFiles = [
  { name: '_md-sys-shape.scss', outName: '_md-sys-shape.css', prefix: '--md-sys-shape-' },
  { name: '_md-sys-motion.scss', outName: '_md-sys-motion.css', prefix: '--md-sys-motion-' },
  { name: '_md-sys-elevation.scss', outName: '_md-sys-elevation.css', prefix: '--md-sys-elevation-' },
  { name: '_md-sys-state.scss', outName: '_md-sys-state.css', prefix: '--md-sys-state-' },
  { name: '_md-sys-typescale.scss', outName: '_md-sys-typescale.css', prefix: '--md-sys-typescale-' }
];

let generatedSysCount = 0;
for (const sys of sysStandardFiles) {
  const scssPath = path.join(vDir, sys.name);
  if (!fs.existsSync(scssPath)) continue;

  const content = fs.readFileSync(scssPath, 'utf8');
  const lines = content.split(/\r?\n/);
  const tokens = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const m = line.match(/'([^']+)':/);
    if (m) {
      const key = m[1];
      const combined = lines.slice(i, i + 5).join(' ');
      const vm = combined.match(/null,\s*(\([^)]+\)|#[a-fA-F0-9]+|[0-9a-zA-Z.-]+(?:px|rem|ms|s)?|cubic-bezier\([^)]+\))/);
      let val = 'initial';
      if (vm) {
        val = vm[1].replace(/^\(|\)$/g, '').trim();
      }
      tokens.push({
        token: `${sys.prefix}${key}`,
        value: normalizeFallback(val)
      });
    }
  }

  if (tokens.length > 0) {
    tokens.sort((a, b) => a.token.localeCompare(b.token));
    let css = `/* ==========================================================================
   Material Web System Tokens: ${sys.outName}
   Package: @material/web v${pkgVersion}
   Auto-generated by scripts/extract-material-tokens.mjs. DO NOT EDIT DIRECTLY.
   ========================================================================== */

:root {
`;
    for (const t of tokens) {
      css += `    ${t.token}: ${t.value};\n`;
    }
    css += `}\n`;

    const outPath = path.join(tokensDir, sys.outName);
    fs.writeFileSync(outPath, css, 'utf8');
    generatedSysCount++;
  }
}

// 5. Extract _md-sys-color.css with separated light and dark theme blocks
const colorScssPath = path.join(vDir, '_md-sys-color.scss');
if (fs.existsSync(colorScssPath)) {
  const colorContent = fs.readFileSync(colorScssPath, 'utf8');

  function parseColorTokens(content, funcName) {
    const startIdx = content.indexOf(`function ${funcName}`);
    if (startIdx === -1) return [];
    const endIdx = content.indexOf(');', startIdx);
    const block = content.substring(startIdx, endIdx);
    const tokens = [];
    const lines = block.split(/\r?\n/);
    for (let i = 0; i < lines.length; i++) {
      const line = lines[i];
      const km = line.match(/'([^']+)':/);
      if (km) {
        const combined = lines.slice(i, i + 3).join(' ');
        const vm = combined.match(/'md-ref-palette',\s*'([^']+)'/);
        if (vm) {
          tokens.push({
            token: `--md-sys-color-${km[1]}`,
            value: `var(--md-ref-palette-${vm[1]})`
          });
        }
      }
    }
    return tokens;
  }

  const darkTokens = parseColorTokens(colorContent, 'values-dark');
  const lightTokens = parseColorTokens(colorContent, 'values-light');

  darkTokens.sort((a, b) => a.token.localeCompare(b.token));
  lightTokens.sort((a, b) => a.token.localeCompare(b.token));

  let colorCss = `/* ==========================================================================
   Material Web System Tokens: _md-sys-color.css
   Package: @material/web v${pkgVersion}
   Auto-generated by scripts/extract-material-tokens.mjs. DO NOT EDIT DIRECTLY.
   ========================================================================== */

/* Light Theme Scheme */
:root, [data-theme="light"] {
`;
  for (const t of lightTokens) {
    colorCss += `    ${t.token}: ${t.value};\n`;
  }
  colorCss += `}\n\n/* Dark Theme Scheme */\n:root[data-theme="dark"], [data-theme="dark"] {\n`;
  for (const t of darkTokens) {
    colorCss += `    ${t.token}: ${t.value};\n`;
  }
  colorCss += `}\n`;

  fs.writeFileSync(path.join(tokensDir, '_md-sys-color.css'), colorCss, 'utf8');
  generatedSysCount++;
}

console.log(`✅ Generated ${generatedSysCount} system token files in wwwroot/css/tokens/`);

// 6. Extract reference token files (_md-ref-palette.css, _md-ref-typeface.css)
const refPalettePath = path.join(vDir, '_md-ref-palette.scss');
if (fs.existsSync(refPalettePath)) {
  const content = fs.readFileSync(refPalettePath, 'utf8');
  const lines = content.split(/\r?\n/);
  const paletteTokens = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const km = line.match(/'([^']+)':/);
    if (km) {
      const combined = lines.slice(i, i + 3).join(' ');
      const vm = combined.match(/null,\s*(#[a-fA-F0-9]+)/);
      if (vm) {
        paletteTokens.push({
          token: `--md-ref-palette-${km[1]}`,
          value: normalizeFallback(vm[1])
        });
      }
    }
  }

  paletteTokens.sort((a, b) => a.token.localeCompare(b.token));
  let palCss = `/* ==========================================================================
   Material Web Reference Tokens: _md-ref-palette.css
   Package: @material/web v${pkgVersion}
   Auto-generated by scripts/extract-material-tokens.mjs. DO NOT EDIT DIRECTLY.
   ========================================================================== */

:root {
`;
  for (const t of paletteTokens) {
    palCss += `    ${t.token}: ${t.value};\n`;
  }
  palCss += `}\n`;
  fs.writeFileSync(path.join(tokensDir, '_md-ref-palette.css'), palCss, 'utf8');
  console.log(`✅ Generated _md-ref-palette.css (${paletteTokens.length} palette tokens)`);
}

const refTypefacePath = path.join(vDir, '_md-ref-typeface.scss');
if (fs.existsSync(refTypefacePath)) {
  const content = fs.readFileSync(refTypefacePath, 'utf8');
  const lines = content.split(/\r?\n/);
  const typeTokens = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const km = line.match(/'([^']+)':/);
    if (km) {
      const combined = lines.slice(i, i + 3).join(' ');
      const vm = combined.match(/null,\s*(\([^)]+\)|[0-9a-zA-Z.]+)/);
      if (vm) {
        typeTokens.push({
          token: `--md-ref-typeface-${km[1]}`,
          value: normalizeFallback(vm[1].replace(/^\(|\)$/g, ''))
        });
      }
    }
  }

  typeTokens.sort((a, b) => a.token.localeCompare(b.token));
  let typeCss = `/* ==========================================================================
   Material Web Reference Tokens: _md-ref-typeface.css
   Package: @material/web v${pkgVersion}
   Auto-generated by scripts/extract-material-tokens.mjs. DO NOT EDIT DIRECTLY.
   ========================================================================== */

:root {
`;
  for (const t of typeTokens) {
    typeCss += `    ${t.token}: ${t.value};\n`;
  }
  typeCss += `}\n`;
  fs.writeFileSync(path.join(tokensDir, '_md-ref-typeface.css'), typeCss, 'utf8');
  console.log(`✅ Generated _md-ref-typeface.css (${typeTokens.length} typeface tokens)`);
}

console.log(`🎉 Token extraction complete: all files mirrored with original @material/web names.`);
