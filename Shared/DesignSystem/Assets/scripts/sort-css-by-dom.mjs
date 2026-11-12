import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import postcss from 'postcss';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const cssRoot = path.resolve(__dirname, '../wwwroot/css');
const targetDirs = ['layouts', 'foundations/grid.css', 'components', 'widgets', 'pages'];

// Top-level document landmark ranking (1st level under body)
const LANDMARK_RANKS = {
  'body': 5,
  'layout': 8,
  'header': 10,
  'nav': 20,
  'main': 30,
  'section': 40,
  'article': 50,
  'aside': 60,
  'footer': 70,
  'dialog': 80,
  'menu': 90
};

// Internal semantic element ranking within containers
const ELEMENT_RANKS = {
  // Top badges / indicators / media
  'mark': 10,
  'figure': 20,
  'img': 22,
  'svg': 24,
  'video': 26,
  'canvas': 28,

  // Headings & typography
  'header': 30,
  'hgroup': 32,
  'h1': 33,
  'h2': 34,
  'h3': 35,
  'h4': 36,
  'h5': 37,
  'h6': 38,
  'p': 40,
  'span': 42,
  'strong': 43,
  'em': 44,
  'code': 45,
  'pre': 46,
  'samp': 47,
  'data': 48,
  'output': 49,

  // Navigation, tabs & chips
  'nav': 50,
  'md-tabs': 52,
  'md-primary-tab': 53,
  'md-secondary-tab': 54,
  'md-chip-set': 56,
  'md-chip': 57,
  'md-filter-chip': 58,
  'ul': 60,
  'ol': 62,
  'li': 64,
  'dl': 66,
  'dt': 67,
  'dd': 68,

  // Content organisms & cards
  'section': 70,
  'article': 72,
  'md-card': 74,
  'md-filled-card': 75,
  'md-outlined-card': 76,
  'md-elevated-card': 77,

  // Tables
  'table': 80,
  'thead': 81,
  'tbody': 82,
  'tfoot': 83,
  'tr': 84,
  'th': 85,
  'td': 86,

  // Forms & interactive controls
  'form': 90,
  'fieldset': 91,
  'legend': 92,
  'label': 93,
  'input': 94,
  'md-filled-text-field': 95,
  'md-outlined-text-field': 96,
  'md-select': 97,
  'md-filled-select': 98,
  'md-outlined-select': 99,
  'md-checkbox': 100,
  'md-radio': 101,
  'md-switch': 102,
  'md-slider': 103,

  // Aside / auxiliary sidebar
  'aside': 110,

  // Footers, action menus & buttons
  'footer': 120,
  'menu': 125,
  'button': 130,
  'md-filled-button': 132,
  'md-outlined-button': 134,
  'md-text-button': 136,
  'md-elevated-button': 138,
  'md-icon-button': 140,
  'md-fab': 142,
  'md-icon': 144,
  'a': 146,
  'md-divider': 150
};

export function parseSelectorPath(sel) {
  const first = sel.split(',')[0].trim();
  const rawParts = first.split('>').map(p => p.trim());

  return rawParts.map(part => {
    const match = part.match(/^([a-z0-9-]+)/i);
    const tag = match ? match[1].toLowerCase() : part;

    const attrMatch = part.match(/\[([a-zA-Z0-9_-]+)(?:=[^\]]+)?\]/);
    const attr = attrMatch ? attrMatch[1] : '';

    let pseudoType = 0; // 0=none, 1=pseudo-element, 2=structural, 3=state
    if (part.includes('::')) pseudoType = 1;
    else if (part.includes(':hover') || part.includes(':active') || part.includes(':focus')) pseudoType = 3;
    else if (part.includes(':')) pseudoType = 2;

    return { raw: part, tag, attr, pseudoType };
  });
}

export function compareNodes(pathA, pathB) {
  const minLen = Math.min(pathA.length, pathB.length);

  for (let i = 0; i < minLen; i++) {
    const a = pathA[i];
    const b = pathB[i];

    if (a.raw !== b.raw) {
      if (a.tag !== b.tag) {
        if (i === 0) {
          const rankA = LANDMARK_RANKS[a.tag] || 999;
          const rankB = LANDMARK_RANKS[b.tag] || 999;
          if (rankA !== rankB) return rankA - rankB;
        }

        const rankA = ELEMENT_RANKS[a.tag] || 999;
        const rankB = ELEMENT_RANKS[b.tag] || 999;
        if (rankA !== rankB) return rankA - rankB;

        return a.tag.localeCompare(b.tag);
      }

      if (a.attr !== b.attr) {
        return a.attr.localeCompare(b.attr);
      }

      if (a.pseudoType !== b.pseudoType) {
        return a.pseudoType - b.pseudoType;
      }

      return a.raw.localeCompare(b.raw);
    }
  }

  // Shorter path (parent) comes before longer path (child)
  return pathA.length - pathB.length;
}

export function sortContainer(container) {
  const groups = [];
  let pendingComments = [];

  for (const node of [...container.nodes]) {
    if (node.type === 'comment') {
      if (node.text.includes('====') && groups.length === 0 && pendingComments.length === 0) {
        groups.push({ type: 'banner', node, comments: [] });
      } else {
        pendingComments.push(node);
      }
    } else if (node.type === 'rule') {
      const path = parseSelectorPath(node.selector);
      groups.push({
        type: 'rule',
        node,
        path,
        comments: pendingComments
      });
      pendingComments = [];
    } else if (node.type === 'atrule') {
      if (node.name === 'media' || node.name === 'container' || node.name === 'layer') {
        sortContainer(node);
        groups.push({
          type: 'atrule',
          node,
          name: node.name,
          params: node.params,
          comments: pendingComments
        });
      } else {
        groups.push({
          type: 'other-atrule',
          node,
          name: node.name,
          comments: pendingComments
        });
      }
      pendingComments = [];
    }
  }

  const banners = groups.filter(g => g.type === 'banner');
  const rules = groups.filter(g => g.type === 'rule');
  const layerAtrules = groups.filter(g => g.type === 'atrule' && g.name === 'layer');
  const otherAtrules = groups.filter(g => g.type === 'other-atrule');
  const mediaAtrules = groups.filter(g => g.type === 'atrule' && (g.name === 'media' || g.name === 'container'));
  const trailingComments = pendingComments;

  rules.sort((a, b) => compareNodes(a.path, b.path));

  mediaAtrules.sort((a, b) => {
    const maxA = a.params ? (a.params.match(/max-width:\s*([\d.]+)px/)?.[1] || 0) : 0;
    const maxB = b.params ? (b.params.match(/max-width:\s*([\d.]+)px/)?.[1] || 0) : 0;
    return Number(maxB) - Number(maxA);
  });

  container.removeAll();

  for (const b of banners) {
    container.append(b.node);
  }

  for (const l of layerAtrules) {
    for (const c of l.comments) container.append(c);
    container.append(l.node);
  }

  let isFirst = banners.length === 0 && layerAtrules.length === 0;
  for (const r of rules) {
    for (const c of r.comments) {
      c.raws.before = isFirst ? '\n' : '\n\n';
      container.append(c);
      isFirst = false;
    }
    r.node.raws.before = isFirst ? '\n' : (r.comments.length > 0 ? '\n' : '\n\n');
    container.append(r.node);
    isFirst = false;
  }

  for (const o of otherAtrules) {
    for (const c of o.comments) container.append(c);
    container.append(o.node);
  }

  for (const m of mediaAtrules) {
    for (const c of m.comments) {
      c.raws.before = '\n\n';
      container.append(c);
    }
    m.node.raws.before = '\n\n';
    container.append(m.node);
  }

  for (const c of trailingComments) {
    c.raws.before = '\n\n';
    container.append(c);
  }
}

export function sortCssFile(filePath, checkOnly = false) {
  const original = fs.readFileSync(filePath, 'utf8');
  let root;
  try {
    root = postcss.parse(original);
  } catch (err) {
    console.error(`❌ Syntax error in ${filePath}:`, err.message);
    return false;
  }

  sortContainer(root);
  const sorted = root.toString().trim() + '\n';

  if (sorted === original) {
    return true; // Already sorted
  }

  if (checkOnly) {
    return false; // Out of order
  }

  fs.writeFileSync(filePath, sorted, 'utf8');
  return true;
}

// CLI Execution
const args = process.argv.slice(2);
const isCheck = args.includes('--check');
const targetArg = args.find(a => !a.startsWith('--'));

function findCssFiles(dir) {
  const results = [];
  if (!fs.existsSync(dir)) return results;
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...findCssFiles(fullPath));
    } else if (entry.isFile() && entry.name.endsWith('.css') && !entry.name.endsWith('.min.css')) {
      results.push(fullPath);
    }
  }
  return results;
}

let files = [];
if (targetArg) {
  const resolved = path.resolve(process.cwd(), targetArg);
  if (fs.existsSync(resolved)) {
    if (fs.statSync(resolved).isDirectory()) {
      files = findCssFiles(resolved);
    } else {
      files.push(resolved);
    }
  } else {
    console.error(`Target path not found: ${targetArg}`);
    process.exit(1);
  }
} else {
  // Collect all files in targetDirs recursively
  for (const d of targetDirs) {
    const targetPath = path.join(cssRoot, d);
    if (fs.existsSync(targetPath)) {
      if (fs.statSync(targetPath).isDirectory()) {
        files.push(...findCssFiles(targetPath));
      } else if (targetPath.endsWith('.css')) {
        files.push(targetPath);
      }
    }
  }
}

console.log(`🌲 [Socratic DOM-CSS Sorter] ${isCheck ? 'Checking' : 'Sorting'} ${files.length} stylesheet(s)...`);

let sortedCount = 0;
let modifiedCount = 0;
let errors = 0;

for (const f of files) {
  const rel = path.relative(cssRoot, f);
  const ok = sortCssFile(f, isCheck);

  if (isCheck) {
    if (ok) {
      sortedCount++;
    } else {
      console.log(`  ⚠️ Out of DOM order: ${rel}`);
      errors++;
    }
  } else {
    modifiedCount++;
    console.log(`  ✅ Sorted: ${rel}`);
  }
}

if (isCheck) {
  if (errors > 0) {
    console.log(`\n❌ ${errors} file(s) are not ordered by DOM pre-order hierarchy.`);
    process.exit(1);
  } else {
    console.log(`\n🎉 All ${files.length} stylesheet(s) are perfectly ordered by DOM pre-order hierarchy!`);
  }
} else {
  console.log(`\n✨ Successfully sorted ${modifiedCount} file(s) by DOM pre-order hierarchy.`);
}

