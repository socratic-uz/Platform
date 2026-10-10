/**
 * Socratic Design System - Strict CSS Value & Syntax Validator
 * Detects malformed values, dangling slashes, empty functions, and syntax errors.
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import postcss from 'postcss';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');
const cssRootDir = path.join(assetsRoot, 'wwwroot', 'css');

function getAllCssFiles(dirPath, fileList = []) {
  if (!fs.existsSync(dirPath)) return fileList;
  const items = fs.readdirSync(dirPath, { withFileTypes: true });
  for (const item of items) {
    const fullPath = path.join(dirPath, item.name);
    if (item.isDirectory()) {
      if (['node_modules', 'fonts'].includes(item.name)) continue;
      getAllCssFiles(fullPath, fileList);
    } else if (item.isFile() && item.name.endsWith('.css') && !item.name.includes('bundle.min')) {
      fileList.push(fullPath);
    }
  }
  return fileList;
}

export function validateCssFiles() {
  const files = getAllCssFiles(cssRootDir);
  const issues = [];

  for (const file of files) {
    const relFile = path.relative(assetsRoot, file).replace(/\\/g, '/');
    const content = fs.readFileSync(file, 'utf8');

    let root;
    try {
      root = postcss.parse(content, { from: file });
    } catch (err) {
      issues.push({
        file: relFile,
        line: err.line || 1,
        column: err.column || 1,
        prop: '',
        value: '',
        message: `Fatal syntax error: ${err.reason || err.message}`
      });
      continue;
    }

    root.walkDecls(decl => {
      const val = decl.value;
      const prop = decl.prop;
      const line = decl.source?.start?.line || 1;
      const col = decl.source?.start?.column || 1;

      // 1. Incomplete relative color: rgb(from var(...) r g b / ) -> missing alpha!
      if (/\/\s*\)/.test(val) || /\/\s*$/.test(val)) {
        issues.push({
          file: relFile,
          line,
          column: col,
          prop,
          value: val,
          message: 'Malformed color: dangling slash without alpha value (/ )'
        });
      }

      // 2. Empty function calls: var(), rgb(), calc()
      if (/\b(rgb|rgba|hsl|hsla|var|calc|clamp|min|max)\(\s*\)/.test(val)) {
        issues.push({
          file: relFile,
          line,
          column: col,
          prop,
          value: val,
          message: 'Empty CSS function call'
        });
      }

      // 3. Semicolon leaked into property value (excluding data: URIs and SVG URLs)
      if (/;/.test(val) && !/url\s*\(\s*(['"]?)data:/i.test(val)) {
        issues.push({
          file: relFile,
          line,
          column: col,
          prop,
          value: val,
          message: 'Semicolon inside declaration value'
        });
      }

      // 4. Dangling comma: e.g. calc(10px, ) or var(--foo, )
      // Note: var(--foo, ) is technically a fallback of empty value in CSS custom properties,
      // but in standard properties (e.g. rgb(1, 2, )) it is invalid.
      if (/(?<!var\([^)]*),\s*\)/.test(val)) {
        issues.push({
          file: relFile,
          line,
          column: col,
          prop,
          value: val,
          message: 'Dangling comma before closing parenthesis'
        });
      }
    });
  }

  return issues;
}

// CLI execution
if (process.argv[1] === fileURLToPath(import.meta.url)) {
  console.log('\n🔍 [CSS Syntax Validator] Scanning all stylesheets for syntax anomalies and malformed values...');
  const issues = validateCssFiles();

  if (issues.length === 0) {
    console.log('🎉 100% Valid: No syntax anomalies or malformed CSS values found!\n');
    process.exit(0);
  } else {
    console.log(`\n❌ Found ${issues.length} syntax issues in CSS stylesheets:\n`);
    for (const issue of issues) {
      console.log(`  🚩 ${issue.file}:${issue.line}:${issue.column}`);
      console.log(`     Property: ${issue.prop}`);
      console.log(`     Value:    ${issue.value}`);
      console.log(`     Issue:    ${issue.message}\n`);
    }
    process.exit(1);
  }
}
