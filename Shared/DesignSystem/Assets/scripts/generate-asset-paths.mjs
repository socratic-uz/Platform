import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import zlib from 'node:zlib';
import crypto from 'node:crypto';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');

const assetsWwwroot = path.join(assetsRoot, 'wwwroot');
const constantsDir = path.join(assetsRoot, 'Constants');
if (!fs.existsSync(constantsDir)) {
  fs.mkdirSync(constantsDir, { recursive: true });
}
const outputCsPath = path.join(constantsDir, 'Assets.g.cs');
const manifestJsonPath = path.join(assetsWwwroot, 'assets-manifest.json');

function toPascalCase(str) {
  if (!str) return 'Root';
  const withoutExt = str.replace(/\.[^/.]+$/, '');
  const cleaned = withoutExt.replace(/[^a-zA-Z0-9]+/g, ' ').trim();
  if (!cleaned) return 'Root';
  const words = cleaned.split(/\s+/);
  let result = words.map(w => w.charAt(0).toUpperCase() + w.slice(1)).join('');
  if (/^[0-9]/.test(result)) {
    result = '_' + result;
  }
  return result;
}

function readFileWithRetry(filePath, retries = 5, delayMs = 60) {
  for (let i = 0; i < retries; i++) {
    try {
      return fs.readFileSync(filePath);
    } catch (err) {
      if ((err.code === 'EBUSY' || err.code === 'EPERM') && i < retries - 1) {
        const start = Date.now();
        while (Date.now() - start < delayMs) { /* busy wait */ }
        continue;
      }
      throw err;
    }
  }
}

function scanAssets() {
  console.log(`\n📦 [Assets Pipeline] Scanning static assets in: ${assetsWwwroot}`);

  const manifestItems = [];
  const categorized = {
    Images: {},
    Scripts: {},
    ScriptsByDomain: {
      Pages: {},
      Widgets: {},
      Layouts: {},
      Features: {},
      Components: {}
    },
    Styles: {},
    Fonts: {},
    Maps: {},
    Audio: {},
    Root: {}
  };

  const IGNORED_EXTENSIONS = new Set(['.br', '.gz', '.map', '.md', '.html', '.blueprint', '.layout']);
  const IGNORED_FILES = new Set(['assets-manifest.json', 'rollup.config.js']);
  const IGNORED_DIRS = new Set(['preview', 'analytics']);

  function walk(currentDir, relativePrefix = '') {
    if (!fs.existsSync(currentDir)) return;
    const entries = fs.readdirSync(currentDir, { withFileTypes: true });

    for (const entry of entries) {
      const fullPath = path.join(currentDir, entry.name);
      const relPath = relativePrefix ? `${relativePrefix}/${entry.name}` : entry.name;

      if (entry.isDirectory()) {
        if (IGNORED_DIRS.has(entry.name.toLowerCase())) {
          continue;
        }
        walk(fullPath, relPath);
        continue;
      }

      const ext = path.extname(entry.name).toLowerCase();
      if (IGNORED_EXTENSIONS.has(ext) || IGNORED_FILES.has(entry.name) || entry.name.endsWith('.config.js')) {
        continue;
      }

      // Compute hash for manifest
      const content = readFileWithRetry(fullPath);
      const hash = crypto.createHash('sha256').update(content).digest('hex').substring(0, 12);
      manifestItems.push({
        url: `_content/Assets/${relPath.replace(/\\/g, '/')}`,
        size: content.length,
        hash: `sha256-${hash}`
      });

      const webPath = `_content/Assets/${relPath.replace(/\\/g, '/')}`;
      const propName = toPascalCase(entry.name);

      const parts = relPath.split('/');
      const topDir = parts[0].toLowerCase();
      if (topDir === 'img' || topDir === 'icons') {
        categorized.Images[propName] = webPath;
      } else if (topDir === 'js') {
        // Blazor dynamic JS module imports (IJSRuntime.InvokeAsync<IJSObjectReference>("import", ...))
        // require a relative path prefix ('./') in the browser to avoid being rejected as bare module specifiers.
        const scriptWebPath = `./${webPath}`;
        categorized.Scripts[propName] = scriptWebPath;

        if (parts.length > 2) {
          const subDir = parts[1].toLowerCase();
          if (subDir === 'pages') {
            categorized.ScriptsByDomain.Pages[propName] = scriptWebPath;
          } else if (subDir === 'widgets') {
            categorized.ScriptsByDomain.Widgets[propName] = scriptWebPath;
          } else if (subDir === 'layouts') {
            categorized.ScriptsByDomain.Layouts[propName] = scriptWebPath;
          } else if (subDir === 'features') {
            categorized.ScriptsByDomain.Features[propName] = scriptWebPath;
          } else if (subDir === 'components') {
            categorized.ScriptsByDomain.Components[propName] = scriptWebPath;
          }
        }
      } else if (topDir === 'css') {
          if (entry.name.includes('bundle') || entry.name === 'styles.css') {
            categorized.Styles[propName] = webPath;
          }
        } else if (topDir === 'fonts') {
          categorized.Fonts[propName] = webPath;
        } else if (topDir === 'maps') {
          categorized.Maps[propName] = webPath;
        } else if (topDir === 'audio') {
          categorized.Audio[propName] = webPath;
        } else {
          categorized.Root[propName] = webPath;
        }
      }
    }

    walk(assetsWwwroot);

    // Write manifest.json
    fs.writeFileSync(manifestJsonPath, JSON.stringify({ version: '1.0', assets: manifestItems }, null, 2), 'utf8');
    console.log(`✅ [Assets Pipeline] PWA cache manifest generated: ${manifestItems.length} assets indexed.`);

    // Generate C# Code
    function renderClass(className, items) {
      const keys = Object.keys(items).sort();
      if (keys.length === 0) return '';
      let code = `    public static class ${className}\n    {\n`;
      for (const key of keys) {
        let safeKey = key;
        if (safeKey === className) {
          safeKey = `${safeKey}File`;
          code += `        public const string Master = "${items[key]}";\n`;
        }
        code += `        public const string ${safeKey} = "${items[key]}";\n`;
      }
      code += `    }\n\n`;
      return code;
    }

    function renderScriptsWithDomains() {
      let code = `    public static class Scripts\n    {\n`;

      // Sub-domain classes mirroring CSS
      for (const [domainName, items] of Object.entries(categorized.ScriptsByDomain)) {
        const keys = Object.keys(items).sort();
        if (keys.length > 0) {
          code += `        public static class ${domainName}\n        {\n`;
          for (const key of keys) {
            code += `            public const string ${key} = "${items[key]}";\n`;
          }
          code += `        }\n\n`;
        }
      }

      // Flat backward-compatible constants
      const flatKeys = Object.keys(categorized.Scripts).sort();
      for (const key of flatKeys) {
        let safeKey = key;
        if (safeKey === 'Scripts') {
          safeKey = 'ScriptsFile';
          code += `        public const string Master = "${categorized.Scripts[key]}";\n`;
        }
        code += `        public const string ${safeKey} = "${categorized.Scripts[key]}";\n`;
      }

      code += `    }\n\n`;
      return code;
    }

    let cs = `// <auto-generated/>
// This file was automatically generated by generate-asset-paths.mjs.
// Strongly-typed static asset paths for the unified Assets package.
// Categories are first-class static classes directly in namespace Assets.Constants.
// Usage in Razor: @Images.LandingHeroMain, @Scripts.Pages.Landing, @Scripts.Widgets.ChatInput.

#nullable enable
#pragma warning disable

namespace Assets.Constants
{
${renderClass('Images', categorized.Images)}${renderScriptsWithDomains()}${renderClass('Styles', categorized.Styles)}${renderClass('Fonts', categorized.Fonts)}${renderClass('Maps', categorized.Maps)}${renderClass('Audio', categorized.Audio)}`;

    const rootKeys = Object.keys(categorized.Root).sort();
    if (rootKeys.length > 0) {
      cs += `    public static class AppManifest\n    {\n`;
      for (const key of rootKeys) {
        cs += `        public const string ${key} = "${categorized.Root[key]}";\n`;
      }
      cs += `    }\n\n`;
    }
    cs += `}\n`;

    fs.writeFileSync(outputCsPath, cs, 'utf8');
    console.log(`✅ [Assets Pipeline] C# asset constants generated: ${outputCsPath}`);
  }

  scanAssets();
