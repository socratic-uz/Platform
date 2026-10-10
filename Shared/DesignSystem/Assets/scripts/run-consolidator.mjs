/**
 * Socratic Platform - AST Consolidator Pipeline CLI
 * CLI Entrypoint for Single-Declaration consolidation and LightningCSS bundling.
 *
 * Usage:
 *   node scripts/run-consolidator.mjs --build
 *   node scripts/run-consolidator.mjs --watch
 *   node scripts/run-consolidator.mjs --audit-current
 *   node scripts/run-consolidator.mjs --consolidate-current
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawn } from 'node:child_process';
import postcss from 'postcss';
import { AstConsolidator } from './consolidator-engine.mjs';
import { validateCssFiles } from './validate-syntax.mjs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');
const cssRootDir = path.join(assetsRoot, 'wwwroot', 'css');
const componentsDir = path.join(cssRootDir, 'components');

const args = process.argv.slice(2);
const isWatch = args.includes('--watch');
const isAudit = args.includes('--audit-current');
const isCleanCurrent = args.includes('--consolidate-current');
const isFromComponents = args.includes('--from-components') || args.includes('--components');

function getAllCssFiles(dirPath, fileList = []) {
  if (!fs.existsSync(dirPath)) return fileList;
  const items = fs.readdirSync(dirPath, { withFileTypes: true });
  for (const item of items) {
    const fullPath = path.join(dirPath, item.name);
    if (item.isDirectory()) {
      // Skip node_modules, tokens, components (they are standalone/deconsolidated)
      if (['node_modules', 'tokens', 'components'].includes(item.name)) {
        continue;
      }
      getAllCssFiles(fullPath, fileList);
    } else if (item.isFile() && item.name.endsWith('.css') && !item.name.includes('bundle.min') && item.name !== 'styles.css') {
      fileList.push(fullPath);
    }
  }
  return fileList;
}

function runBundleScript() {
  return new Promise((resolve, reject) => {
    const bundleProcess = spawn('node', ['scripts/bundle-css.mjs'], {
      cwd: assetsRoot,
      stdio: 'inherit'
    });
    bundleProcess.on('close', code => {
      if (code === 0) resolve();
      else reject(new Error(`bundle-css.mjs exited with code ${code}`));
    });
  });
}

/**
 * Audit existing CSS files for accidental duplicate property declarations
 */
function auditCurrentCss() {
  console.log('\n🔍 [AST Audit] Scanning existing W3C CSS modules for duplicate declarations (scoped)...');
  const files = getAllCssFiles(cssRootDir);
  let totalDuplicates = 0;
  let auditedFiles = 0;

  for (const file of files) {
    const content = fs.readFileSync(file, 'utf8');
    const relPath = path.relative(cssRootDir, file);

    const root = postcss.parse(content, { from: file });
    // Scope -> Map<declKey, count>
    const scopesMap = new Map();
    let fileDuplicates = 0;

    root.walk(node => {
      // Ignore keyframes content
      if (node.type === 'atrule' && (node.name === 'keyframes' || node.name === '-webkit-keyframes')) {
        return;
      }

      if (node.type === 'rule') {
        if (node.parent && node.parent.type === 'atrule' && node.parent.name.includes('keyframes')) {
          return;
        }

        const atRuleStack = [];
        let currentParent = node.parent;
        while (currentParent && currentParent.type === 'atrule') {
          atRuleStack.unshift(`@${currentParent.name} ${currentParent.params.trim()}`);
          currentParent = currentParent.parent;
        }
        const atRuleScope = atRuleStack.join(' { ');

        if (!scopesMap.has(atRuleScope)) {
          scopesMap.set(atRuleScope, new Map());
        }

        const declMap = scopesMap.get(atRuleScope);

        node.walkDecls(decl => {
          const declKey = `${decl.prop.trim().toLowerCase()}: ${decl.value.trim()}${decl.important ? ' !important' : ''}`;
          const currentCount = declMap.get(declKey) || 0;
          if (currentCount >= 1) {
            fileDuplicates++;
          }
          declMap.set(declKey, currentCount + 1);
        });
      }
    });

    if (fileDuplicates > 0) {
      console.log(`  ⚠️  ${relPath}: ${fileDuplicates} redundant duplicate declarations detected.`);
      totalDuplicates += fileDuplicates;
    }
    auditedFiles++;
  }

  console.log(`\n📊 Audit Summary: ${auditedFiles} files scanned, ${totalDuplicates} duplicates found.`);
  if (totalDuplicates > 0) {
    console.log(`💡 Run 'node scripts/run-consolidator.mjs --consolidate-current' to mathematically merge them.`);
  } else {
    console.log(`🎉 100% Pure: Every declaration is unique per scope (Single-Declaration standard satisfied).`);
  }
}

/**
 * Ingest existing W3C CSS modules and re-consolidate them to guarantee 100% uniqueness
 */
async function consolidateCurrentCss() {
  console.log('\n⚡ [AST Consolidator] Re-consolidating current W3C modules to eliminate all duplicates...');
  const files = getAllCssFiles(cssRootDir);
  const consolidator = new AstConsolidator({ outputDir: cssRootDir });

  for (const file of files) {
    const content = fs.readFileSync(file, 'utf8');
    consolidator.processCss(content, file);
  }

  const { totalFiles, totalDeclarations } = consolidator.emitFiles();
  console.log(`✅ Successfully consolidated: ${totalFiles} modules generated with ${totalDeclarations} canonical declarations.`);

  console.log('\n📦 Rebuilding production CSS bundle...');
  await runBundleScript();
}

/**
 * Ingest component stylesheets from wwwroot/css/components/ and consolidate into W3C architecture
 */
async function consolidateFromComponents() {
  console.log('\n⚡ [AST Consolidator] Consolidating styles from wwwroot/css/components/ into W3C architecture...');

  // 1. Strict syntax & value validation check
  const syntaxIssues = validateCssFiles();
  if (syntaxIssues.length > 0) {
    console.error(`\n❌ [AST Consolidator] Consolidation aborted! Found ${syntaxIssues.length} syntax error(s):`);
    for (const issue of syntaxIssues) {
      console.error(`  🚩 ${issue.file}:${issue.line}:${issue.column} - ${issue.prop}: ${issue.value} (${issue.message})`);
    }
    throw new Error(`Consolidation halted: ${syntaxIssues.length} syntax error(s) must be fixed first.`);
  }

  const consolidator = new AstConsolidator({ outputDir: cssRootDir });

  if (!fs.existsSync(componentsDir)) {
    console.error(`❌ Components directory not found: ${componentsDir}`);
    return;
  }

  const componentFiles = fs.readdirSync(componentsDir)
    .filter(f => f.endsWith('.css') && f !== 'components.css' && !f.includes('bundle.min'))
    .map(f => path.join(componentsDir, f));

  console.log(`📥 Ingesting ${componentFiles.length} component stylesheets from ${path.relative(assetsRoot, componentsDir)}...`);
  for (const file of componentFiles) {
    const content = fs.readFileSync(file, 'utf8');
    consolidator.processCss(content, file);
  }

  const { totalFiles, totalDeclarations } = consolidator.emitFiles();
  console.log(`✅ Successfully consolidated into ${totalFiles} W3C modules (${totalDeclarations} canonical declarations).`);

  console.log('\n📦 Rebuilding production CSS bundles via LightningCSS...');
  await runBundleScript();
}

/**
 * Watch mode for real-time development on wwwroot/css/components/
 */
function watchMode() {
  console.log(`\n👀 [AST Consolidator Watch] Watching for component CSS changes in: ${path.relative(assetsRoot, componentsDir)}...`);

  let debounceTimer = null;
  fs.watch(componentsDir, { recursive: true }, (eventType, filename) => {
    if (!filename || !filename.endsWith('.css') || filename === 'components.css' || filename.includes('bundle.min')) return;

    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(async () => {
      console.log(`\n🔄 Component file changed: ${filename}. Consolidating to W3C...`);
      try {
        await consolidateFromComponents();
        console.log(`✨ Re-consolidation complete at ${new Date().toLocaleTimeString()}`);
      } catch (err) {
        console.error(`❌ Compilation error:`, err);
      }
    }, 150);
  });
}

// Execution router
if (isAudit) {
  auditCurrentCss();
} else if (isCleanCurrent) {
  consolidateCurrentCss().catch(err => {
    console.error('Fatal error during consolidation:', err);
    process.exit(1);
  });
} else if (isWatch) {
  watchMode();
} else {
  consolidateFromComponents().catch(err => {
    console.error('Fatal error during component consolidation:', err);
    process.exit(1);
  });
}

