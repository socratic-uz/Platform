/**
 * Socratic Design System — Classless Migration Analyzer
 *
 * Finds `class=@Const` usages on semantic HTML tags where the class can be dropped
 * in favour of the Classless-First foundation (foundations/base.css).
 *
 * Categories:
 *   dead       — class has no CSS rule at all → attribute is pure noise.
 *   redundant  — every declaration of the class is already provided by the tag default
 *                (or is a no-op reset) and the class is never used as a hook in
 *                compound / descendant selectors.
 *
 * Usage: node scripts/analyze-classless.mjs [--json]
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import postcss from 'postcss';
import selectorParser from 'postcss-selector-parser';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const assetsDir = path.resolve(__dirname, '..');
const cssRoot = path.join(assetsDir, 'wwwroot', 'css');
const frontendRoot = path.resolve(assetsDir, '..', '..', '..', '..');
const classesFile = path.join(assetsDir, 'Constants', 'Classes.g.cs');
const reportFile = path.join(assetsDir, 'wwwroot', 'analytics', 'usage', 'classless-candidates.json');

/* Tags whose styling is NOT gated by :not([class]) → removing the class keeps the global default. */
const ungatedTags = new Set(['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'small', 'strong', 'b', 'code', 'kbd', 'mark', 'pre', 'blockquote', 'hr', 'dl', 'dt', 'dd', 'figure', 'figcaption', 'span', 'li', 'em']);

/* Declarations that equal the foundation defaults for a given tag (prop → value). */
const headingFont = (scale) => ({
  'font-size': `var(--md-sys-typescale-${scale}-size)`,
  'font-weight': `var(--md-sys-typescale-${scale}-weight)`,
  'line-height': `var(--md-sys-typescale-${scale}-line-height)`,
  'letter-spacing': `var(--md-sys-typescale-${scale}-tracking)`
});
const common = { margin: '0', 'margin-top': '0', 'margin-bottom': '0', 'margin-block': '0' };
const heading = { ...common, color: 'var(--md-sys-color-on-surface)', 'font-family': 'var(--md-ref-typeface-brand)', 'overflow-wrap': 'anywhere', 'text-wrap': 'balance' };
const tagDefaults = {
  h1: { ...heading, ...headingFont('display-large') },
  h2: { ...heading, ...headingFont('headline-large') },
  h3: { ...heading, ...headingFont('headline-medium') },
  h4: { ...heading, ...headingFont('title-large') },
  h5: { ...heading, ...headingFont('title-medium') },
  h6: { ...heading, 'font-weight': '600' },
  p: { ...common, 'text-wrap': 'pretty' },
  small: { 'font-size': 'var(--md-sys-typescale-body-small-size)', color: 'var(--md-sys-color-on-surface-variant)' },
  strong: { 'font-weight': '600' },
  figure: { margin: '0', display: 'grid' },
  dl: { margin: '0', display: 'grid' }
};

/* Tags whose rich defaults are gated by :not([class]) → dropping the class restyles the element. */
const gatedTags = new Set(['article', 'aside', 'nav', 'section', 'hgroup', 'ul', 'ol', 'table', 'fieldset', 'form', 'label', 'button', 'input', 'select', 'textarea', 'details', 'dialog']);

const norm = (v) => v.replace(/\s+/g, ' ').replace(/,\s*[^)]*\)$/, (m) => m).trim().toLowerCase();
const stripFallback = (v) => {
  const m = /^var\((--[\w-]+)\s*,.*\)$/.exec(v.trim());
  return m ? `var(${m[1]})` : v.trim();
};

/* ---------- 1. Classes.g.cs → group → const → css class ---------- */
function parseClassConstants() {
  const src = fs.readFileSync(classesFile, 'utf8');
  const groups = new Map();
  let current = null;
  for (const line of src.split(/\r?\n/)) {
    const g = /public static class (\w+)/.exec(line);
    if (g && g[1] !== 'Classes') { current = g[1]; if (!groups.has(current)) groups.set(current, new Map()); continue; }
    const c = /public const string (\w+) = "([^"]+)"/.exec(line);
    if (c && current) groups.get(current).set(c[1], c[2]);
  }
  return groups;
}

/* ---------- 2. CSS → per class: simple rules + hook usage ---------- */
function walk(dir, ext, out = []) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) { if (!['bin', 'obj', 'node_modules', 'fonts', '.git'].includes(e.name)) walk(p, ext, out); }
    else if (e.name.endsWith(ext)) out.push(p);
  }
  return out;
}

function parseCss() {
  const info = new Map(); // class → { decls: Map(prop→value), simple: n, hooks: n, files:Set }
  const tagProps = new Map(); // tag → Set(props) set by non-foundation rules whose subject is that tag
  const get = (c) => { if (!info.has(c)) info.set(c, { decls: new Map(), declNodes: [], simple: 0, hooks: 0, atRule: 0, files: new Set() }); return info.get(c); };
  for (const file of walk(cssRoot, '.css')) {
    const rel = path.relative(cssRoot, file).replace(/\\/g, '/');
    const isFoundation = rel.startsWith('foundations/') || rel.startsWith('tokens/');
    const root = postcss.parse(fs.readFileSync(file, 'utf8'));
    root.walkRules((rule) => {
      if (rule.parent?.type === 'atrule' && /keyframes/i.test(rule.parent.name)) return;
      const inAt = rule.parent?.type === 'atrule';
      selectorParser((sels) => {
        sels.each((sel) => {
          // subject tag of the selector (last compound) → props it sets
          if (!isFoundation) {
            const nodes = sel.nodes;
            let i = nodes.length - 1;
            while (i >= 0 && nodes[i].type !== 'combinator') {
              if (nodes[i].type === 'tag') {
                const t = nodes[i].value.toLowerCase();
                if (!tagProps.has(t)) tagProps.set(t, new Set());
                rule.walkDecls((d) => tagProps.get(t).add(d.prop.toLowerCase()));
              }
              i--;
            }
          }
          const classes = [];
          sel.walkClasses((n) => classes.push(n.value));
          if (!classes.length) return;
          // Unwrap :where(.x) / :is(.x)
          const str = sel.toString().trim().replace(/^:(where|is)\((.*)\)$/, '$2').trim();
          const simple = /^(?:[a-z][\w-]*)?\.[\w-]+$/i.test(str);
          for (const c of classes) {
            const i = get(c);
            i.files.add(rel);
            if (simple && !inAt) {
              i.simple++;
              rule.walkDecls((d) => { i.decls.set(d.prop.toLowerCase(), d.value); i.declNodes.push({ file: rel, line: d.source.start.line, prop: d.prop.toLowerCase(), value: d.value, shared: rule.selectors.length > 1 }); });
            } else if (simple && inAt) {
              i.atRule++;
            } else {
              i.hooks++;
            }
          }
        });
      }).processSync(rule.selector);
    });
  }
  return { info, tagProps };
}

/* ---------- 3. Razor usages ---------- */
function parseRazor(groups) {
  const constToGroups = new Map();
  for (const [g, map] of groups) for (const k of map.keys()) {
    if (!constToGroups.has(k)) constToGroups.set(k, []);
    constToGroups.get(k).push(g);
  }
  const usages = [];
  const tagRe = /<([a-z][a-z0-9]*)\b([^>]*?)\sclass=@([A-Za-z_]\w*)(?=[\s/>])/g;
  const allRefs = new Map(); // constName → total textual references (razor + cs, excluding generated)
  const sources = [...walk(frontendRoot, '.razor'), ...walk(frontendRoot, '.cs').filter((f) => !f.endsWith('.g.cs'))];
  for (const file of sources) {
    for (const m of fs.readFileSync(file, 'utf8').matchAll(/\b([A-Z]\w*)\b/g)) allRefs.set(m[1], (allRefs.get(m[1]) || 0) + 1);
  }
  usages.allRefs = allRefs;
  for (const file of walk(frontendRoot, '.razor')) {
    const text = fs.readFileSync(file, 'utf8');
    const usingGroups = [...text.matchAll(/@using static (?:Assets\.Constants\.)?Classes\.(\w+)/g)].map((m) => m[1]);
    const lines = text.split(/\r?\n/);
    lines.forEach((line, idx) => {
      for (const m of line.matchAll(tagRe)) {
        const [, tag, , constName] = m;
        const candidates = (constToGroups.get(constName) || []).filter((g) => usingGroups.includes(g));
        const cssClass = candidates.length ? groups.get(candidates[0]).get(constName) : null;
        usages.push({ file: path.relative(frontendRoot, file).replace(/\\/g, '/'), line: idx + 1, tag, constName, cssClass, text: line.trim() });
      }
    });
  }
  return usages;
}

/* ---------- 4. Classification ---------- */
function main() {
  const groups = parseClassConstants();
  const { info: css, tagProps } = parseCss();
  const parsed = parseRazor(groups);
  const allRefs = parsed.allRefs;
  const usages = parsed.filter((u) => u.cssClass);

  // tag distribution per class (across ALL usages incl. non-semantic)
  const byClass = new Map();
  for (const u of usages) {
    if (!byClass.has(u.cssClass)) byClass.set(u.cssClass, []);
    byClass.get(u.cssClass).push(u);
  }

  const dead = [];
  const redundant = [];
  const redundantDecls = [];
  for (const [cls, list] of byClass) {
    const i = css.get(cls);
    if (!i) {
      dead.push({ cls, restyles: list.some((u) => gatedTags.has(u.tag)), usages: list });
      continue;
    }
    if (i.hooks || i.atRule) continue;
    // every reference to the constant must be one of the counted tag usages (no Class=@X, string.Format, C# refs)
    if ((allRefs.get(list[0].constName) || 0) > list.length) continue;
    if (/^m3-/.test(cls)) continue; // shared typography / utility primitives
    const tags = new Set(list.map((u) => u.tag));
    if (tags.size !== 1) continue;
    const tag = [...tags][0];
    const defaults = tagDefaults[tag];
    if (!defaults) continue;
    const contested = tagProps.get(tag) || new Set();
    const isDup = (prop, value) => defaults[prop] !== undefined && !contested.has(prop) && norm(stripFallback(value)) === norm(defaults[prop]);
    const extra = [...i.decls].filter(([p, v]) => !isDup(p, v));
    if (!extra.length) {
      redundant.push({ cls, tag, decls: Object.fromEntries(i.decls), files: [...i.files], usages: list });
    } else {
      const dups = i.declNodes.filter((d) => !d.shared && isDup(d.prop, d.value));
      if (dups.length) redundantDecls.push({ cls, tag, decls: dups });
    }
  }

  const report = {
    generatedAt: new Date().toISOString(),
    totals: { usages: usages.length, dead: dead.length, redundant: redundant.length, redundantDecls: redundantDecls.reduce((s, r) => s + r.decls.length, 0) },
    dead, redundant, redundantDecls
  };
  fs.mkdirSync(path.dirname(reportFile), { recursive: true });
  fs.writeFileSync(reportFile, JSON.stringify(report, null, 2));

  console.log(`\n🧹 [Classless Analyzer] ${usages.length} class=@Const usages scanned`);
  console.log(`   dead (no CSS rule)          : ${dead.length} classes / ${dead.reduce((s, d) => s + d.usages.length, 0)} usages`);
  console.log(`   redundant class (tag default): ${redundant.length} classes / ${redundant.reduce((s, d) => s + d.usages.length, 0)} usages`);
  console.log(`   redundant declarations      : ${report.totals.redundantDecls} in ${redundantDecls.length} classes`);
  if (process.argv.includes('--verbose')) {
    for (const d of dead) console.log(`   ✗ dead${d.restyles ? ' (restyles)' : ''} .${d.cls} ← ${d.usages.map((u) => `<${u.tag}> ${u.file}:${u.line}`).join(', ')}`);
    for (const r of redundant) console.log(`   ≈ class .${r.cls} on <${r.tag}> [${Object.keys(r.decls).join(', ') || 'empty'}] ← ${r.usages.length} usages`);
    for (const r of redundantDecls) console.log(`   − decl  .${r.cls} on <${r.tag}>: ${r.decls.map((d) => `${d.prop} (${d.file}:${d.line})`).join(', ')}`);
  }
  console.log(`   📄 ${path.relative(assetsDir, reportFile)}\n`);
}

main();
