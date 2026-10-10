import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

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

function fixContent(content) {
  const targetPrefix = 'rgb(from ';
  const targetSuffix = ' r g b / )';
  let idx = 0;
  let result = '';
  let replacements = 0;

  while (idx < content.length) {
    const start = content.indexOf(targetPrefix, idx);
    if (start === -1) {
      result += content.slice(idx);
      break;
    }

    result += content.slice(idx, start);
    let p = start + targetPrefix.length;
    let depth = 0;
    const varStart = p;
    while (p < content.length) {
      if (content[p] === '(') depth++;
      else if (content[p] === ')') {
        depth--;
        if (depth === 0) {
          p++;
          break;
        }
      }
      p++;
    }

    const varExpr = content.slice(varStart, p);
    const rest = content.slice(p);
    if (rest.startsWith(targetSuffix)) {
      result += varExpr;
      idx = p + targetSuffix.length;
      replacements++;
    } else {
      result += content.slice(start, p);
      idx = p;
    }
  }

  return { result, replacements };
}

const files = getAllCssFiles(cssRootDir);
let totalFixed = 0;

for (const file of files) {
  const content = fs.readFileSync(file, 'utf8');
  const { result, replacements } = fixContent(content);

  if (replacements > 0) {
    fs.writeFileSync(file, result, 'utf8');
    console.log(`✅ Fixed ${replacements} syntax errors in ${path.relative(assetsRoot, file)}`);
    totalFixed += replacements;
  }
}

console.log(`\n🎉 Total syntax errors repaired: ${totalFixed}`);
