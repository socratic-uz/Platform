import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');

const cssRoot = path.join(assetsRoot, 'wwwroot', 'css');
const scanDirs = [
  path.join(cssRoot, 'box-model'),
  path.join(cssRoot, 'layout'),
  path.join(cssRoot, 'positioning'),
  path.join(cssRoot, 'overflow'),
  path.join(cssRoot, 'typography'),
  path.join(cssRoot, 'color'),
  path.join(cssRoot, 'effects'),
  path.join(cssRoot, 'transforms'),
  path.join(cssRoot, 'motion'),
  path.join(cssRoot, 'content'),
  path.join(cssRoot, 'interaction'),
  path.join(cssRoot, 'i18n'),
  path.join(cssRoot, 'containment'),
  path.join(cssRoot, 'tokens'),
];

const constantsDir = path.join(assetsRoot, 'Constants');
if (!fs.existsSync(constantsDir)) {
  fs.mkdirSync(constantsDir, { recursive: true });
}
const outputFilePath = path.join(constantsDir, 'Classes.g.cs');

function toPascalCase(str) {
  if (!str) return 'Root';
  const cleaned = str.replace(/[^a-zA-Z0-9]+/g, ' ').trim();
  if (!cleaned) return 'Root';
  const words = cleaned.split(/\s+/);
  let result = words.map(w => w.charAt(0).toUpperCase() + w.slice(1)).join('');
  if (/^[0-9]/.test(result)) {
    result = '_' + result;
  }
  return result;
}

function extractClassesFromCss(cssContent) {
  const classRegex = /\.([a-zA-Z_-][a-zA-Z0-9_-]*)/g;
  const classes = new Set();
  let match;
  while ((match = classRegex.exec(cssContent)) !== null) {
    const className = match[1];
    // Filter out internal pseudo, numbers, web components, and invalid identifiers
    if (className && !className.startsWith('md-') && className !== 'css' && !/^\d/.test(className)) {
      classes.add(className);
    }
  }
  return Array.from(classes).sort();
}

function resolveCanonicalDomainGroup(cls) {
  const c = cls.toLowerCase();
  if (c.includes('card')) return 'Cards';
  if (c.includes('picker') || c.startsWith('lang-') || c.includes('swatch')) return 'Picker';
  if (c.includes('table') || c.startsWith('col-') || c.startsWith('row-') || c.includes('cell') || c.includes('paginator') || c === 'quickgrid') return 'Table';
  if (c.includes('dialog') || c.includes('modal')) return 'Dialog';
  if (c.includes('badge') || c.includes('counter')) return 'Badge';
  if (c.includes('chip')) return 'Chip';
  if (c.includes('avatar') || c.includes('contact')) return 'Avatar';
  if (c.includes('skeleton')) return 'Skeleton';
  if (c.includes('progress')) return 'Progress';
  if (c.includes('segmented')) return 'SegmentedButton';
  if (c.includes('search')) return 'Search';
  if (c.includes('empty')) return 'EmptyState';
  if (c.includes('info')) return 'InfoGrid';
  if (c.includes('toggle') || c.includes('mode-')) return 'ThemeToggle';
  if (c.includes('field') || c.includes('form') || c.includes('drop-container')) return 'Form';
  if (c.includes('scrollbar') || c.includes('scroll')) return 'Scrollbar';
  if (c.includes('focus-ring') || c.includes('outline')) return 'Outline';
  if (c.includes('button') || c.startsWith('btn-')) return 'Buttons';
  return null;
}

function getAllCssFiles(dir) {
  let results = [];
  if (!fs.existsSync(dir)) return results;
  const list = fs.readdirSync(dir, { withFileTypes: true });
  for (const dirent of list) {
    const fullPath = path.join(dir, dirent.name);
    if (dirent.isDirectory()) {
      results = results.concat(getAllCssFiles(fullPath));
    } else if (dirent.isFile() && dirent.name.endsWith('.css')) {
      results.push(fullPath);
    }
  }
  return results;
}

function generate() {
  console.log(`\n⚙️  [CSS-to-C# Generator] Scanning stylesheets with strict deduplication...`);

  const groups = new Map();
  const assignedClasses = new Set();

  // 1. Load canonical component snapshot (preserves 100% backward compatibility for Blazor)
  const snapshotPath = path.join(__dirname, 'component-classes-snapshot.json');
  if (fs.existsSync(snapshotPath)) {
    const snap = JSON.parse(fs.readFileSync(snapshotPath, 'utf8'));
    for (const [grp, pMap] of Object.entries(snap)) {
      if (!groups.has(grp)) groups.set(grp, new Map());
      const gMap = groups.get(grp);
      for (const [p, val] of Object.entries(pMap)) {
        gMap.set(p, val);
        assignedClasses.add(val);
      }
    }
  }

  // 2. Load legacy aliases (Geometry, Typography, Motion)
  const legacyAliases = {
    Geometry: {
      ConfigDialog: 'm3-config-dialog',
      DialogSurface: 'm3-dialog-surface',
      NoScrollbar: 'no-scrollbar',
      PageGutter: 'm3-page-gutter',
      RatioCard: 'm3-ratio-card',
      RatioKiosk: 'm3-ratio-kiosk',
      RatioPortrait: 'm3-ratio-portrait',
      RatioSquare: 'm3-ratio-square',
      RatioVideo: 'm3-ratio-video',
      RatioWide: 'm3-ratio-wide',
      ScrollbarNone: 'scrollbar-none',
      ScrollbarThin: 'scrollbar-thin',
      SectionGutter: 'm3-section-gutter',
      TouchTarget: 'm3-touch-target',
      TouchTargetKiosk: 'm3-touch-target-kiosk',
    },
    Typography: {
      BodyLarge: 'm3-body-large',
      BodyMedium: 'm3-body-medium',
      DisplayLarge: 'm3-display-large',
      HeadlineLarge: 'm3-headline-large',
      HeadlineMedium: 'm3-headline-medium',
      LabelSmall: 'm3-label-small',
      LineClamp2: 'm3-line-clamp-2',
      LineClamp3: 'm3-line-clamp-3',
      TitleLarge: 'm3-title-large',
      TitleMedium: 'm3-title-medium',
      Truncate: 'm3-truncate',
    },
    Motion: {
      HoverLift: 'm3-hover-lift',
      PressScale: 'm3-press-scale',
    },
  };

  for (const [legacyGroup, props] of Object.entries(legacyAliases)) {
    if (!groups.has(legacyGroup)) groups.set(legacyGroup, new Map());
    const gMap = groups.get(legacyGroup);
    for (const [p, val] of Object.entries(props)) {
      gMap.set(p, val);
    }
  }

  // 3. Scan W3C modules and assign each unassigned class to exactly ONE group
  for (const dir of scanDirs) {
    if (!fs.existsSync(dir)) continue;
    const files = getAllCssFiles(dir);

    for (const filePath of files) {
      const content = fs.readFileSync(filePath, 'utf-8');
      const baseName = path.basename(filePath, '.css');
      const classes = extractClassesFromCss(content);

      for (const cls of classes) {
        if (assignedClasses.has(cls)) continue;

        // Determine destination group
        let targetGroup = resolveCanonicalDomainGroup(cls);
        if (!targetGroup) {
          targetGroup = toPascalCase(baseName);
        }

        if (!groups.has(targetGroup)) {
          groups.set(targetGroup, new Map());
        }
        const gMap = groups.get(targetGroup);

        // Compute property name
        let propName = cls;
        const lowerGrp = targetGroup.toLowerCase().replace(/s$/, '');
        const prefixPatterns = [
          new RegExp(`^m3-${lowerGrp}-`),
          new RegExp(`^m3-${lowerGrp}s-`),
          /^m3-/,
          new RegExp(`^${lowerGrp}-`),
        ];
        for (const pat of prefixPatterns) {
          if (pat.test(propName)) {
            propName = propName.replace(pat, '');
            break;
          }
        }
        let finalProp = toPascalCase(propName);
        if (!finalProp || finalProp === targetGroup) finalProp = 'Root';

        // Disambiguate if needed
        let uniqueProp = finalProp;
        let counter = 2;
        while (gMap.has(uniqueProp) && gMap.get(uniqueProp) !== cls) {
          uniqueProp = `${finalProp}_${counter++}`;
        }

        gMap.set(uniqueProp, cls);
        assignedClasses.add(cls);
      }
    }
  }

  // 4. Generate C# code
  const sortedGroups = Array.from(groups.keys()).sort();

  let body = '';
  for (const groupName of sortedGroups) {
    const propMap = groups.get(groupName);
    if (!propMap || propMap.size === 0) continue;

    body += `        public static class ${groupName}\n        {\n`;

    const sortedProps = Array.from(propMap.entries()).sort((a, b) => a[0].localeCompare(b[0]));
    for (const [propName, rawClass] of sortedProps) {
      const escaped = rawClass.replace(/"/g, '\\"');
      body += `            public const string ${propName} = "${escaped}";\n`;
    }

    body += `        }\n\n`;
  }
  body = body.trimEnd();

  let csharp = `// <auto-generated/>
// This file was automatically generated by generate-css-classes.mjs.
// Strongly-typed CSS class constants for unified Assets Design System.
// Design System CSS Dictionary (Public API - Fully Deduplicated).

#nullable enable
#pragma warning disable

namespace Assets.Constants
{
    /// <summary>
    /// Strongly-typed CSS class selectors for the unified Assets Design System.
    /// Provides compile-time safety, zero constant duplication, and IDE IntelliSense.
    /// </summary>
    public static class Classes
    {
${body}
    }
}

namespace Styles
{
    /// <summary>
    /// Backward-compatibility surface for Styles.CssClasses.
    /// </summary>
    public static class CssClasses
    {
${body}
    }
}

namespace Material.Web
{
    /// <summary>
    /// Strongly-typed CSS class selectors for Material Design 3 Blazor components.
    /// Backward-compatibility surface.
    /// </summary>
    public static class M3
    {
${body}
    }
}
`;

  fs.mkdirSync(path.dirname(outputFilePath), { recursive: true });
  fs.writeFileSync(outputFilePath, csharp, 'utf-8');
  console.log(`✅ Generated strongly-typed C# classes at: ${outputFilePath}`);
  console.log(`   Total distinct component groups: ${sortedGroups.length}`);
  console.log(`   Total unique CSS classes registered: ${assignedClasses.size}`);
}

generate();
