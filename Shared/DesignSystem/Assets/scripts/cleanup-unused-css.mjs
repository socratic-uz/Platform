import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import postcss from 'postcss';
import selectorParser from 'postcss-selector-parser';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsDir = path.resolve(__dirname, '..');
const cssRoot = path.join(assetsDir, 'wwwroot', 'css');
const reportPath = path.join(assetsDir, 'wwwroot', 'analytics', 'usage', 'unused-css-classes.json');
const snapshotPath = path.join(__dirname, 'component-classes-snapshot.json');
const writeChanges = process.argv.includes('--write');
const verbose = process.argv.includes('--verbose');
const runtimeGeneratedCssClasses = new Set([
  'actor',
  'edgePaths',
  'components-reconnect-failed',
  'components-reconnect-paused',
  'components-reconnect-resume-failed',
  'components-reconnect-retrying',
  'components-reconnect-show',
  'mapboxgl-ctrl-group',
  'mapboxgl-ctrl-top-right',
  'maplibregl-ctrl-group',
  'maplibregl-ctrl-top-right',
  'material-symbols-outlined',
  'yandex-ctrl-icon'
]);

function findCssFiles(dir) {
  const files = [];
  if (!fs.existsSync(dir)) return files;

  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const filePath = path.join(dir, entry.name);
    if (entry.isDirectory()) files.push(...findCssFiles(filePath));
    else if (entry.isFile() && entry.name.endsWith('.css')) files.push(filePath);
  }

  return files;
}

function hasRequiredUnusedClass(selector, unusedClasses, protectedClasses) {
  let found = false;
  selector.walkClasses((node) => {
    let parent = node.parent;
    while (parent && parent !== selector) {
      if (parent.type === 'pseudo' && parent.value.toLowerCase() === ':not') return;
      parent = parent.parent;
    }
    if (unusedClasses.has(node.value) && !protectedClasses.has(node.value)) found = true;
  });
  return found;
}

function pruneUnusedSelectorAlternatives(selector, unusedClasses, protectedClasses, removedAlternatives) {
  let removed = 0;
  let invalid = false;
  selector.walkPseudos((pseudo) => {
    if (![':where', ':is', ':matches', ':-webkit-any', ':-moz-any'].includes(pseudo.value.toLowerCase())) return;
    for (const alternative of [...(pseudo.nodes ?? [])]) {
      if (!hasRequiredUnusedClass(alternative, unusedClasses, protectedClasses)) continue;
      removedAlternatives.push(alternative.toString());
      alternative.remove();
      removed++;
    }
    if (pseudo.nodes?.length === 0) {
      pseudo.remove();
      invalid = true;
    }
  });
  return { removed, invalid };
}

function parseSelector(selector, filePath) {
  try {
    return selectorParser().astSync(selector);
  } catch (error) {
    throw new Error(`Unable to parse selector in ${filePath}: ${selector}`, { cause: error });
  }
}

async function run() {
  if (!fs.existsSync(reportPath)) {
    throw new Error('Usage report not found. Run `npm run generate:usage` first.');
  }

  const report = JSON.parse(fs.readFileSync(reportPath, 'utf8'));
  const unusedClasses = new Set((report.unusedCssClasses ?? []).map((entry) => entry.cssClass));
  const protectedClasses = new Set([
    ...runtimeGeneratedCssClasses,
    ...(report.protectedRuntimeCssClasses ?? []).map((entry) => entry.cssClass)
  ]);
  const cssChanges = [];
  const remainingCssClasses = new Set();
  let removedSelectorBranches = 0;
  let removedSelectorAlternatives = 0;
  let removedRules = 0;
  const removedDetails = [];

  for (const filePath of findCssFiles(cssRoot)) {
    const source = fs.readFileSync(filePath, 'utf8');
    const root = postcss.parse(source, { from: filePath });

    root.walkRules((rule) => {
      const selectorRoot = parseSelector(rule.selector, filePath);
      let changed = false;

      for (const selector of [...selectorRoot.nodes]) {
        const removedAlternatives = [];
        const pruning = pruneUnusedSelectorAlternatives(selector, unusedClasses, protectedClasses, removedAlternatives);
        removedSelectorAlternatives += pruning.removed;
        changed ||= pruning.removed > 0;
        if (verbose) {
          for (const alternative of removedAlternatives) removedDetails.push(`${path.relative(cssRoot, filePath)}:${rule.source.start.line} removed alternative ${alternative}`);
        }
        if (!pruning.invalid && !hasRequiredUnusedClass(selector, unusedClasses, protectedClasses)) continue;

        if (verbose) removedDetails.push(`${path.relative(cssRoot, filePath)}:${rule.source.start.line} removed branch ${selector.toString()}`);
        selector.remove();
        changed = true;
        removedSelectorBranches++;
      }

      if (!changed) return;
      if (selectorRoot.nodes.length === 0) {
        rule.remove();
        removedRules++;
      } else {
        rule.selector = selectorRoot.toString();
      }
    });

    root.walkAtRules((atRule) => {
      if (atRule.nodes?.length === 0) atRule.remove();
    });

    root.walkRules((rule) => {
      const selectorRoot = parseSelector(rule.selector, filePath);
      selectorRoot.walkClasses((node) => remainingCssClasses.add(node.value));
    });

    const output = root.toString();
    if (output !== source) cssChanges.push({ filePath, output });
  }

  const removedCssClasses = new Set([...unusedClasses].filter((cssClass) => !remainingCssClasses.has(cssClass)));
  const snapshot = JSON.parse(fs.readFileSync(snapshotPath, 'utf8'));
  let removedSnapshotAliases = 0;
  for (const [group, aliases] of Object.entries(snapshot)) {
    for (const [name, cssClass] of Object.entries(aliases)) {
      if (!removedCssClasses.has(cssClass)) continue;
      delete aliases[name];
      removedSnapshotAliases++;
    }
    if (Object.keys(aliases).length === 0) delete snapshot[group];
  }

  console.log(`CSS files to update: ${cssChanges.length}`);
  console.log(`Unused selector branches to remove: ${removedSelectorBranches}`);
  console.log(`Unused nested selector alternatives to remove: ${removedSelectorAlternatives}`);
  console.log(`CSS classes fully removed: ${removedCssClasses.size}`);
  console.log(`Snapshot aliases to remove: ${removedSnapshotAliases}`);
  if (verbose) for (const detail of removedDetails) console.log(detail);

  if (!writeChanges) {
    console.log('Dry run only. Re-run with --write to apply these changes.');
    return;
  }

  for (const { filePath, output } of cssChanges) fs.writeFileSync(filePath, output, 'utf8');
  fs.writeFileSync(snapshotPath, `${JSON.stringify(snapshot, null, 2)}\n`, 'utf8');
  console.log('Unused CSS selectors and snapshot aliases removed. Regenerate Classes.g.cs next.');
}

run().catch((error) => {
  console.error('CSS cleanup failed:', error);
  process.exitCode = 1;
});
