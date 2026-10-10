/**
 * Socratic Platform - Reverse Deconsolidator Pipeline CLI
 * CLI runner for reconstructing components and unifying tokens.
 *
 * Usage:
 *   node scripts/run-deconsolidator.mjs
 *   node scripts/run-deconsolidator.mjs --tokens-only
 *   node scripts/run-deconsolidator.mjs --components-only
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { AstDeconsolidator } from './deconsolidator-engine.mjs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');
const cssRootDir = path.join(assetsRoot, 'wwwroot', 'css');
const w3cDir = path.join(cssRootDir, 'w3c');
const tokensDir = path.join(cssRootDir, 'tokens');
const componentsDir = path.join(cssRootDir, 'components');

const args = process.argv.slice(2);
const tokensOnly = args.includes('--tokens-only');
const componentsOnly = args.includes('--components-only');

function getAllCssFiles(dir, list = []) {
  if (!fs.existsSync(dir)) return list;
  const items = fs.readdirSync(dir, { withFileTypes: true });
  for (const it of items) {
    const full = path.join(dir, it.name);
    if (it.isDirectory()) {
      getAllCssFiles(full, list);
    } else if (it.isFile() && it.name.endsWith('.css') && !it.name.includes('bundle.min')) {
      list.push(full);
    }
  }
  return list;
}

async function run() {
  console.log('\n🔄 [AST Reverse Engine] Starting Deconsolidation & Reconstruction Pipeline...\n');

  const engine = new AstDeconsolidator({
    w3cDir,
    componentsDir,
    tokensDir
  });

  // 1. Optimize tokens (:root unification)
  if (!componentsOnly) {
    console.log('💎 1. Unifying and optimizing token files in wwwroot/css/tokens/...');
    const { optimizedFiles } = engine.optimizeTokens();
    console.log(`   ✅ Optimized ${optimizedFiles} token stylesheets (all :root blocks unified).\n`);
  }

  // 2. Reconstruct components from W3C modules
  if (!tokensOnly) {
    console.log('🧩 2. Ingesting W3C modules and reconstructing components in wwwroot/css/components/...');

    const w3cFiles = getAllCssFiles(w3cDir);
    console.log(`   📥 Loading ${w3cFiles.length} W3C single-declaration modules...`);
    for (const file of w3cFiles) {
      const content = fs.readFileSync(file, 'utf8');
      engine.processW3cCss(content, file);
    }

    const { totalComponentFiles, totalClasses, generatedFiles } = engine.emitComponents();
    console.log(`   ✅ Successfully reconstructed ${totalComponentFiles} component stylesheets:`);
    for (const f of generatedFiles) {
      console.log(`      • wwwroot/css/components/${f}`);
    }
    console.log(`      • wwwroot/css/components/components.css (master index)`);
    console.log(`   🎉 Total unique component classes/selectors unified: ${totalClasses}\n`);
  }

  console.log('✨ Deconsolidation completed successfully!');
}

run().catch(err => {
  console.error('Fatal error in reverse deconsolidator:', err);
  process.exit(1);
});
