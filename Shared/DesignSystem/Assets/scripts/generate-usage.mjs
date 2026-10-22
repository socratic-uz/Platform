import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const assetsDir = path.resolve(__dirname, '..');
const wwwrootDir = path.resolve(assetsDir, 'wwwroot');
const analyticsDir = path.resolve(wwwrootDir, 'analytics');
const usageDir = path.resolve(analyticsDir, 'usage');
const classesCsPath = path.resolve(assetsDir, 'Constants/Classes.g.cs');
const locCsPath = path.resolve(assetsDir, 'Constants/Loc.g.cs');
const repoFrontendRoot = path.resolve(assetsDir, '../../../../');

if (!fs.existsSync(usageDir)) {
  fs.mkdirSync(usageDir, { recursive: true });
}

// -------------------------------------------------------------
// 1b. Loc.g.cs Parsing
// -------------------------------------------------------------
function parseLocFile(filePath) {
  if (!fs.existsSync(filePath)) {
    console.warn(`⚠️ Loc.g.cs not found at ${filePath}`);
    return { locConstantsMap: new Map(), rawKeyToLocMap: new Map(), domainKeysMap: new Map() };
  }

  const content = fs.readFileSync(filePath, 'utf8');
  const lines = content.split('\n');

  let currentDomain = null;
  const locConstantsMap = new Map(); // key: "Domain.Name", value: { domain, name, locKey, count: 0, files: Set }
  const rawKeyToLocMap = new Map();  // key: locKey, value: ["Domain.Name"]
  const domainKeysMap = new Map();   // key: domain, value: Set of names

  const domainDefRegex = /public\s+static\s+class\s+([A-Za-z0-9_]+)/;
  const constDefRegex = /public\s+const\s+string\s+([A-Za-z0-9_]+)\s*=\s*"([^"]+)";/;

  for (const line of lines) {
    const classMatch = line.match(domainDefRegex);
    if (classMatch) {
      if (classMatch[1] === 'Loc') {
        currentDomain = null;
      } else {
        currentDomain = classMatch[1];
        if (!domainKeysMap.has(currentDomain)) {
          domainKeysMap.set(currentDomain, new Set());
        }
      }
      continue;
    }

    if (currentDomain) {
      const constMatch = line.match(constDefRegex);
      if (constMatch) {
        const name = constMatch[1];
        const locKey = constMatch[2];
        const key = `${currentDomain}.${name}`;

        const record = {
          domain: currentDomain,
          name,
          locKey,
          count: 0,
          files: new Set()
        };
        locConstantsMap.set(key, record);
        domainKeysMap.get(currentDomain).add(name);

        if (!rawKeyToLocMap.has(locKey)) {
          rawKeyToLocMap.set(locKey, []);
        }
        rawKeyToLocMap.get(locKey).push(key);
      }
    }
  }

  return { locConstantsMap, rawKeyToLocMap, domainKeysMap };
}

// -------------------------------------------------------------
// 1. Classes.g.cs Parsing
// -------------------------------------------------------------
function parseClassesFile(filePath) {
  if (!fs.existsSync(filePath)) {
    console.warn(`⚠️ Classes.g.cs not found at ${filePath}`);
    return { constantsMap: new Map(), cssToConstantMap: new Map() };
  }

  const content = fs.readFileSync(filePath, 'utf8');
  const lines = content.split('\n');

  let currentCategory = null;
  const constantsMap = new Map(); // key: "Category.Name", value: { category, name, cssClass, count: 0, files: Set }
  const cssToConstantMap = new Map(); // key: cssClass, value: ["Category.Name"]

  const classDefRegex = /public\s+static\s+class\s+([A-Za-z0-9_]+)/;
  const constDefRegex = /public\s+const\s+string\s+([A-Za-z0-9_]+)\s*=\s*"([^"]+)";/;

  for (const line of lines) {
    const classMatch = line.match(classDefRegex);
    if (classMatch && classMatch[1] !== 'Classes') {
      currentCategory = classMatch[1];
      continue;
    }

    if (currentCategory) {
      const constMatch = line.match(constDefRegex);
      if (constMatch) {
        const name = constMatch[1];
        const cssClass = constMatch[2];
        const key = `${currentCategory}.${name}`;

        const record = {
          category: currentCategory,
          name,
          cssClass,
          count: 0,
          files: new Set()
        };
        constantsMap.set(key, record);

        if (!cssToConstantMap.has(cssClass)) {
          cssToConstantMap.set(cssClass, []);
        }
        cssToConstantMap.get(cssClass).push(key);
      }
    }
  }

  return { constantsMap, cssToConstantMap };
}

// -------------------------------------------------------------
// 2. Discover Razor and C# Source Files
// -------------------------------------------------------------
function findSourceFiles(rootDir) {
  const targetDirs = ['Platform', 'Retail', 'Portal', 'Studio', 'Intelligence'];
  const results = [];
  const ignorePatterns = ['node_modules', 'bin', 'obj', '.git', 'preview', 'Constants', 'dist'];

  function walk(currentDir) {
    if (!fs.existsSync(currentDir)) return;
    const entries = fs.readdirSync(currentDir, { withFileTypes: true });

    for (const entry of entries) {
      if (entry.isDirectory()) {
        if (ignorePatterns.includes(entry.name)) continue;
        walk(path.join(currentDir, entry.name));
      } else if (entry.isFile()) {
        const ext = path.extname(entry.name).toLowerCase();
        if (ext === '.razor' || ext === '.cs') {
          results.push(path.join(currentDir, entry.name));
        }
      }
    }
  }

  for (const dirName of targetDirs) {
    walk(path.join(rootDir, dirName));
  }

  return results;
}

// -------------------------------------------------------------
// 3. Static Analysis (Razor/C# AST & Text Scanner)
// -------------------------------------------------------------
function analyzeSourceFiles(files, constantsMap, cssToConstantMap, locConstantsMap, rawKeyToLocMap, domainKeysMap) {
  const directClassesUsage = new Map(); // cssClass -> { count, files: Set, recommendedConstant }
  const webComponentsMap = new Map(); // tagName -> { count, files: Set }
  const directLocUsage = new Map(); // locKey -> { count, files: Set, recommendedConstant }

  const constantRegex = /Classes\.([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)/g;
  const webComponentRegex = /<(md-[a-z0-9-]+)[\s>/]/g;
  const strRegex = /"([^"\r\n\\]*)"|'([^'\r\n\\]*)'/g;

  // Localization regexes
  const typedLocRegex = /Loc\.([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)/g;
  const usingStaticLocRegex = /(?:@using|using)\s+static\s+Assets\.Constants\.Loc\.([A-Za-z0-9_]+)/g;
  const localizerRawRegex = /(?:L|localizer|_localizer|StringLocalizer)\[\s*["']([^"'\r\n]+)["']\s*\]/g;
  const localizerTypedRegex = /(?:L|localizer|_localizer|StringLocalizer)\[\s*([A-Za-z0-9_]+)\s*\]/g;

  let totalConstantMatches = 0;
  let totalLocMatches = 0;

  for (const filePath of files) {
    let content = '';
    try {
      content = fs.readFileSync(filePath, 'utf8');
    } catch {
      continue;
    }

    const relPath = path.relative(repoFrontendRoot, filePath).replace(/\\/g, '/');
    const isRazor = filePath.endsWith('.razor');

    // 1. Match typed constant calls: Classes.<Category>.<Name> or M3.<Category>.<Name>
    const constantRegex = /(?:Classes|M3)\.([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)/g;
    let match;
    while ((match = constantRegex.exec(content)) !== null) {
      const key = `${match[1]}.${match[2]}`;
      const record = constantsMap.get(key);
      if (record) {
        record.count++;
        record.files.add(relPath);
        totalConstantMatches++;
      }
    }

    // 1b. Match typed localization constants: Loc.<Domain>.<Name>
    let locMatch;
    while ((locMatch = typedLocRegex.exec(content)) !== null) {
      const key = `${locMatch[1]}.${locMatch[2]}`;
      const record = locConstantsMap.get(key);
      if (record) {
        record.count++;
        record.files.add(relPath);
        totalLocMatches++;
      }
    }

    // Check for @using static Assets.Constants.Loc.<Domain>
    const importedDomains = [];
    let domainMatch;
    while ((domainMatch = usingStaticLocRegex.exec(content)) !== null) {
      importedDomains.push(domainMatch[1]);
    }

    if (importedDomains.length > 0) {
      let tMatch;
      while ((tMatch = localizerTypedRegex.exec(content)) !== null) {
        const name = tMatch[1];
        if (name === 'Loc') continue;
        for (const d of importedDomains) {
          if (domainKeysMap.has(d) && domainKeysMap.get(d).has(name)) {
            const key = `${d}.${name}`;
            const record = locConstantsMap.get(key);
            if (record) {
              record.count++;
              record.files.add(relPath);
              totalLocMatches++;
              break;
            }
          }
        }
      }
    }

    // 1c. Match raw string localization calls: L["<key>"]
    let rawLocMatch;
    while ((rawLocMatch = localizerRawRegex.exec(content)) !== null) {
      const rawKey = rawLocMatch[1];
      if (rawKeyToLocMap.has(rawKey)) {
        if (!directLocUsage.has(rawKey)) {
          directLocUsage.set(rawKey, {
            count: 0,
            files: new Set(),
            recommendedConstant: `Loc.${rawKeyToLocMap.get(rawKey)[0]}`
          });
        }
        const entry = directLocUsage.get(rawKey);
        entry.count++;
        entry.files.add(relPath);
      }
    }

    // 2. Accurate detection of raw CSS class string literals in HTML/Blazor class attributes & UI code
    // Only inspect values inside class/CssClass attributes, avoiding slot, Name, Icon, href, etc.
    const classAttrRegex = /(?:(?:^|\s)(?:class|Class|CssClass|HeaderClass|CardClass|BodyClass|TitleClass|IconClass|ContainerClass)\s*=\s*(?:"([^"]*)"|'([^']*)'))|(?:builder\.AddAttribute\s*\(\s*[^,]+,\s*"(?:class|Class|CssClass)"\s*,\s*"([^"]*)"\))/g;
    
    // In .cs files, only scan Razor code-behinds or UI components/layouts
    const isUiCode = isRazor || filePath.endsWith('.razor.cs') || /Component|Layout|Renderer|Widget/i.test(filePath);

    if (isUiCode) {
      let attrMatch;
      while ((attrMatch = classAttrRegex.exec(content)) !== null) {
        const val = attrMatch[1] || attrMatch[2] || attrMatch[3];
        if (!val || val.length > 300) continue;

        // Extract tokens while ignoring @Classes.*, @M3.*, Razor expressions, and CSS variables
        const cleanVal = val.replace(/@(?:Classes|M3)\.[A-Za-z0-9_.]+/g, ' ');
        const tokens = cleanVal.split(/[\s"'{}\(\)\?:@]+/);

        for (const token of tokens) {
          if (!token || token.length < 2 || token.startsWith('var(') || token.startsWith('--') || token.startsWith('http')) continue;
          if (cssToConstantMap.has(token)) {
            if (!directClassesUsage.has(token)) {
              directClassesUsage.set(token, {
                count: 0,
                files: new Set(),
                recommendedConstant: cssToConstantMap.get(token)[0]
              });
            }
            const entry = directClassesUsage.get(token);
            entry.count++;
            entry.files.add(relPath);
          }
        }
      }
    }

    // 3. Detect Material Web Components in Razor markup
    if (isRazor) {
      let wcMatch;
      while ((wcMatch = webComponentRegex.exec(content)) !== null) {
        const tag = wcMatch[1];
        if (!webComponentsMap.has(tag)) {
          webComponentsMap.set(tag, { count: 0, files: new Set() });
        }
        const wcEntry = webComponentsMap.get(tag);
        wcEntry.count++;
        wcEntry.files.add(relPath);
      }
    }
  }

  return { totalConstantMatches, directClassesUsage, webComponentsMap, totalLocMatches, directLocUsage };
}

// -------------------------------------------------------------
// 4. Main Static Usage Analysis Execution
// -------------------------------------------------------------
async function runUsageAnalysis() {
  const startTime = performance.now();
  console.log('📊 [Socratic Static Usage Analyzer] Running pure C# & Razor AST scan...');

  // Step A: Parse Classes.g.cs and Loc.g.cs
  console.log('🔍 Parsing Classes.g.cs & Loc.g.cs constants...');
  const { constantsMap, cssToConstantMap } = parseClassesFile(classesCsPath);
  const { locConstantsMap, rawKeyToLocMap, domainKeysMap } = parseLocFile(locCsPath);

  // Step B: Scan source files
  const sourceFiles = findSourceFiles(repoFrontendRoot);
  console.log(`📁 Scanning ${sourceFiles.length} source files (.razor & .cs)...`);
  const { totalConstantMatches, directClassesUsage, webComponentsMap, totalLocMatches, directLocUsage } = analyzeSourceFiles(
    sourceFiles,
    constantsMap,
    cssToConstantMap,
    locConstantsMap,
    rawKeyToLocMap,
    domainKeysMap
  );

  // Step C: Format and write class-constants.json
  const constantsList = Array.from(constantsMap.values()).map((r) => ({
    key: `${r.category}.${r.name}`,
    category: r.category,
    name: r.name,
    cssClass: r.cssClass,
    usageCountInRazor: r.count,
    filesCount: r.files.size,
    files: Array.from(r.files)
  }));

  const usedConstants = constantsList
    .filter((c) => c.usageCountInRazor > 0)
    .sort((a, b) => b.usageCountInRazor - a.usageCountInRazor);
  const unusedConstants = constantsList
    .filter((c) => c.usageCountInRazor === 0)
    .sort((a, b) => a.key.localeCompare(b.key));

  const byCategory = {};
  for (const c of constantsList) {
    if (!byCategory[c.category]) {
      byCategory[c.category] = { total: 0, used: 0, totalCalls: 0 };
    }
    byCategory[c.category].total++;
    if (c.usageCountInRazor > 0) {
      byCategory[c.category].used++;
      byCategory[c.category].totalCalls += c.usageCountInRazor;
    }
  }

  const rawViolations = Array.from(directClassesUsage.entries())
    .map(([cssClass, data]) => ({
      cssClass,
      occurrences: data.count,
      recommendedConstant: data.recommendedConstant,
      filesCount: data.files.size,
      files: Array.from(data.files)
    }))
    .sort((a, b) => b.occurrences - a.occurrences);

  const totalRawViolations = rawViolations.reduce((sum, v) => sum + v.occurrences, 0);

  const classConstantsReport = {
    generatedAt: new Date().toISOString(),
    summary: {
      totalDeclaredConstants: constantsList.length,
      usedInRazorCount: usedConstants.length,
      unusedCount: unusedConstants.length,
      adoptionRatePercent: Math.round((usedConstants.length / constantsList.length) * 100),
      totalCallSites: totalConstantMatches,
      rawStringViolationsCount: totalRawViolations,
      classesWithRawLiteralsCount: rawViolations.length,
      sourceFilesScanned: sourceFiles.length
    },
    topUsedConstants: usedConstants.slice(0, 50),
    byCategory,
    rawStringViolations: rawViolations.slice(0, 50),
    unusedConstants: unusedConstants.slice(0, 100)
  };

  const classConstantsFile = path.resolve(usageDir, 'class-constants.json');
  fs.writeFileSync(classConstantsFile, JSON.stringify(classConstantsReport, null, 2), 'utf8');

  // Step D: Format and write localization.json
  const locConstantsList = Array.from(locConstantsMap.values()).map((r) => ({
    key: `${r.domain}.${r.name}`,
    domain: r.domain,
    name: r.name,
    locKey: r.locKey,
    usageCountInRazor: r.count,
    filesCount: r.files.size,
    files: Array.from(r.files)
  }));

  const usedLocConstants = locConstantsList
    .filter((c) => c.usageCountInRazor > 0)
    .sort((a, b) => b.usageCountInRazor - a.usageCountInRazor);
  const unusedLocConstants = locConstantsList
    .filter((c) => c.usageCountInRazor === 0)
    .sort((a, b) => a.key.localeCompare(b.key));

  const byDomain = {};
  for (const c of locConstantsList) {
    if (!byDomain[c.domain]) {
      byDomain[c.domain] = { total: 0, used: 0, totalCalls: 0 };
    }
    byDomain[c.domain].total++;
    if (c.usageCountInRazor > 0) {
      byDomain[c.domain].used++;
      byDomain[c.domain].totalCalls += c.usageCountInRazor;
    }
  }

  const rawLocViolations = Array.from(directLocUsage.entries())
    .map(([locKey, data]) => ({
      locKey,
      occurrences: data.count,
      recommendedConstant: data.recommendedConstant,
      filesCount: data.files.size,
      files: Array.from(data.files)
    }))
    .sort((a, b) => b.occurrences - a.occurrences);

  const totalRawLocViolations = rawLocViolations.reduce((sum, v) => sum + v.occurrences, 0);

  const localizationReport = {
    generatedAt: new Date().toISOString(),
    summary: {
      totalDeclaredKeys: locConstantsList.length,
      usedInRazorCount: usedLocConstants.length,
      unusedCount: unusedLocConstants.length,
      adoptionRatePercent: locConstantsList.length > 0 ? Math.round((usedLocConstants.length / locConstantsList.length) * 100) : 0,
      totalTypedCallSites: totalLocMatches,
      rawStringViolationsCount: totalRawLocViolations,
      keysWithRawLiteralsCount: rawLocViolations.length,
      sourceFilesScanned: sourceFiles.length
    },
    topUsedKeys: usedLocConstants.slice(0, 50),
    byDomain,
    rawStringViolations: rawLocViolations.slice(0, 50),
    unusedKeys: unusedLocConstants.slice(0, 100)
  };

  const localizationFile = path.resolve(usageDir, 'localization.json');
  fs.writeFileSync(localizationFile, JSON.stringify(localizationReport, null, 2), 'utf8');

  // Step E: Format and write components.json (statically extracted from Razor)
  const componentsList = Array.from(webComponentsMap.entries())
    .map(([component, data]) => ({
      component,
      count: data.count,
      filesCount: data.files.size,
      files: Array.from(data.files)
    }))
    .sort((a, b) => b.count - a.count);

  const totalWebComponentInstances = componentsList.reduce((sum, c) => sum + c.count, 0);

  const componentsReport = {
    generatedAt: new Date().toISOString(),
    summary: {
      totalWebComponentsUsed: totalWebComponentInstances,
      uniqueComponentsCount: componentsList.length,
      sourceFilesScanned: sourceFiles.length
    },
    webComponents: componentsList
  };

  const componentsFile = path.resolve(usageDir, 'components.json');
  fs.writeFileSync(componentsFile, JSON.stringify(componentsReport, null, 2), 'utf8');

  // Remove obsolete css-selectors.json if present
  const obsoleteSelectorsFile = path.resolve(usageDir, 'css-selectors.json');
  if (fs.existsSync(obsoleteSelectorsFile)) {
    try {
      fs.unlinkSync(obsoleteSelectorsFile);
    } catch {
      // ignore
    }
  }

  const elapsed = Math.round(performance.now() - startTime);

  console.log(`\n✅ [Static Usage Analysis Completed in ${elapsed}ms]`);
  console.log(`   📁 Source files scanned: ${sourceFiles.length} (.razor & .cs)`);
  console.log(`   🏷️  Classes.g.cs constants: ${usedConstants.length} / ${constantsList.length} utilized (${classConstantsReport.summary.adoptionRatePercent}%)`);
  console.log(`   ⚡ Total @Classes.* call-sites: ${totalConstantMatches}`);
  console.log(`   ⚠️  Raw string class occurrences: ${totalRawViolations} across ${rawViolations.length} classes`);
  console.log(`   🌐 Loc.g.cs keys: ${usedLocConstants.length} / ${locConstantsList.length} utilized (${localizationReport.summary.adoptionRatePercent}%)`);
  console.log(`   ⚡ Total typed localization call-sites: ${totalLocMatches}`);
  console.log(`   ⚠️  Raw string localization occurrences: ${totalRawLocViolations} across ${rawLocViolations.length} keys`);
  console.log(`   🧩 Material Web Component tags: ${totalWebComponentInstances} calls across ${componentsList.length} components`);
  console.log(`   💾 Artifacts written:`);
  console.log(`      • analytics/usage/class-constants.json`);
  console.log(`      • analytics/usage/localization.json`);
  console.log(`      • analytics/usage/components.json\n`);

  console.log(`🔝 Top 5 Used Classes.g.cs Constants in Razor:`);
  usedConstants.slice(0, 5).forEach((c) => console.log(`   - @Classes.${c.key}: ${c.usageCountInRazor} calls (across ${c.filesCount} files)`));

  if (rawViolations.length > 0) {
    console.log(`\n⚠️  Top Raw CSS String Classes in Razor (should be migrated to @Classes.*):`);
    rawViolations.slice(0, 5).forEach((v) => console.log(`   - "${v.cssClass}": ${v.occurrences} occurrences (use @Classes.${v.recommendedConstant})`));
  }

  if (rawLocViolations.length > 0) {
    console.log(`\n⚠️  Top Raw Localization Keys in Razor (should be migrated to @L[...]):`);
    rawLocViolations.slice(0, 5).forEach((v) => console.log(`   - L["${v.locKey}"]: ${v.occurrences} occurrences (use @L[${v.recommendedConstant.replace('Loc.', '')}])`));
  }

  if (componentsList.length > 0) {
    console.log(`\n🔝 Top 5 Material Web Components in Razor:`);
    componentsList.slice(0, 5).forEach((c) => console.log(`   - <${c.component}>: ${c.count} times (across ${c.filesCount} files)`));
  }
}

runUsageAnalysis().catch((err) => {
  console.error('Fatal error in static usage analyzer:', err);
  process.exit(1);
});
