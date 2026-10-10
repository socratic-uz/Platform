/**
 * Socratic Platform - Reverse AST Deconsolidator Engine
 * Inverts the W3C Single-Declaration architecture:
 * 1. Collects all properties of each class/selector across all 40 W3C modules
 *    into single, unified rule blocks in wwwroot/css/components/
 * 2. Unifies all declarations of :root inside wwwroot/css/tokens/ into single blocks
 */

import fs from 'node:fs';
import path from 'node:path';
import postcss from 'postcss';

const PROPERTY_ORDER = [
  // Display & Flow
  'display', 'visibility', 'opacity',
  // Layout & Alignment
  'grid', 'grid-template', 'grid-template-columns', 'grid-template-rows', 'grid-template-areas',
  'grid-auto-flow', 'grid-auto-columns', 'grid-auto-rows', 'grid-column', 'grid-row', 'grid-area',
  'subgrid', 'masonry-auto-flow',
  'gap', 'row-gap', 'column-gap',
  'justify-content', 'justify-items', 'justify-self',
  'align-content', 'align-items', 'align-self',
  'place-content', 'place-items', 'place-self',
  // Positioning
  'position', 'inset', 'top', 'right', 'bottom', 'left', 'z-index', 'isolation',
  'anchor-name', 'position-anchor', 'position-area',
  // Sizing & Box Model
  'box-sizing', 'width', 'min-width', 'max-width', 'height', 'min-height', 'max-height',
  'inline-size', 'min-inline-size', 'max-inline-size', 'block-size', 'min-block-size', 'max-block-size',
  'aspect-ratio',
  'margin', 'margin-top', 'margin-right', 'margin-bottom', 'margin-left',
  'margin-inline', 'margin-block',
  'padding', 'padding-top', 'padding-right', 'padding-bottom', 'padding-left',
  'padding-inline', 'padding-block',
  // Border & Outline
  'border', 'border-width', 'border-style', 'border-color',
  'border-top', 'border-right', 'border-bottom', 'border-left',
  'border-radius', 'border-top-left-radius', 'border-top-right-radius', 'border-bottom-left-radius', 'border-bottom-right-radius',
  'border-collapse', 'border-spacing',
  'outline', 'outline-width', 'outline-style', 'outline-color', 'outline-offset',
  // Background & Colors
  'background', 'background-color', 'background-image', 'background-position', 'background-size', 'background-repeat', 'background-clip',
  'color', 'color-scheme',
  // Typography
  'font', 'font-family', 'font-size', 'font-weight', 'font-style', 'line-height',
  'letter-spacing', 'text-align', 'text-decoration', 'text-transform', 'text-indent',
  'white-space', 'text-overflow', 'overflow-wrap', 'word-break',
  // Overflow
  'overflow', 'overflow-x', 'overflow-y', 'scroll-behavior', 'overscroll-behavior', 'scrollbar-width', 'scrollbar-color',
  // Effects
  'box-shadow', 'filter', 'backdrop-filter', 'clip-path',
  // Transforms & Motion
  'transform', 'transform-origin', 'scale', 'rotate', 'translate', 'perspective',
  'will-change',
  'transition', 'transition-property', 'transition-duration', 'transition-timing-function', 'transition-delay',
  'animation', 'animation-name', 'animation-duration', 'animation-timing-function', 'animation-delay',
  // Interaction & UI
  'cursor', 'pointer-events', 'touch-action', 'user-select', 'appearance', 'accent-color', 'resize',
  // Containment
  'contain', 'container-type', 'container-name'
];

function getPropWeight(prop) {
  const p = prop.toLowerCase().trim();
  if (p.startsWith('--')) return 2000;
  const idx = PROPERTY_ORDER.indexOf(p);
  return idx !== -1 ? idx : 1000;
}

export function resolveComponentCategory(selector) {
  const s = selector.toLowerCase().trim();

  // 1. Reset / Base Elements
  if (/^(\*|html|body|:root|img|video|audio|svg|canvas|figure|table|button|input|select|textarea|a|p|h[1-6]|ul|ol|li|dl|dt|dd|details|summary)(:[a-z-]+)?$/.test(s) || s === '::selection') {
    return 'base.css';
  }

  // 2. Expressive M3X
  if (s.includes('m3x-') || s.includes('subgrid-')) {
    return 'expressive.css';
  }

  // 3. Buttons
  if (s.includes('btn') || s.includes('button')) {
    return 'buttons.css';
  }

  // 4. Cards
  if (s.includes('card') || s.includes('bento') || s.includes('tier-box') || s.includes('summary')) {
    return 'cards.css';
  }

  // 5. Dialogs, Modals & Drawers
  if (s.includes('dialog') || s.includes('modal') || s.includes('drawer') || s.includes('backdrop') || s.includes('popover')) {
    return 'dialogs.css';
  }

  // 6. Tables & Lists
  if (s.includes('table') || s.includes('quickgrid') || s.includes('paginator') || s.includes('col-') || s.includes('row-')) {
    return 'tables.css';
  }

  // 7. Forms, Inputs & Pickers
  if (s.includes('field') || s.includes('input') || s.includes('picker') || s.includes('form') || s.includes('select') || s.includes('swatch')) {
    return 'forms.css';
  }

  // 8. Chips & Badges
  if (s.includes('chip') || s.includes('badge') || s.includes('pill') || s.includes('tag')) {
    return 'chips.css';
  }

  // 9. Navigation
  if (s.includes('nav') || s.includes('breadcrumb') || s.includes('tab') || s.includes('menu') || s.includes('top-bar')) {
    return 'navigation.css';
  }

  // 10. Layout, Containers & Shell
  if (s.includes('grid') || s.includes('deck') || s.includes('container') || s.includes('section') || s.includes('wrapper') || s.includes('layout')) {
    return 'layout.css';
  }

  // 11. Common / General
  return 'common.css';
}

export class AstDeconsolidator {
  constructor(options = {}) {
    this.w3cDir = options.w3cDir;
    this.componentsDir = options.componentsDir;
    this.tokensDir = options.tokensDir;
    // Map: selector -> Map<atRuleScope, Map<prop, { value, important }>>
    this.selectorMap = new Map();
    // Keyframes storage
    this.keyframes = new Map();
  }

  /**
   * Process all CSS from W3C modules
   */
  processW3cCss(cssContent, sourceFileName = '') {
    const root = postcss.parse(cssContent, { from: sourceFileName });

    root.walk(node => {
      if (node.type === 'atrule' && (node.name === 'keyframes' || node.name === '-webkit-keyframes')) {
        this.keyframes.set(node.params.trim(), node.toString().trim());
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
        const atRuleScope = atRuleStack.join(' {\n    ');

        // Extract clean atomic selectors
        const selectors = this._extractSelectors(node.selector || '');
        if (selectors.length === 0) return;

        node.walkDecls(decl => {
          const prop = decl.prop.trim().toLowerCase();
          const value = decl.value.trim();
          const important = !!decl.important;

          for (const sel of selectors) {
            this._addProperty(sel, atRuleScope, prop, value, important);
          }
        });
      }
    });
  }

  _extractSelectors(selectorString) {
    function splitTopLevelCommas(s) {
      const list = [];
      let cur = '';
      let depth = 0;
      for (let i = 0; i < s.length; i++) {
        const c = s[i];
        if (c === '(') depth++;
        else if (c === ')') depth--;
        if (c === ',' && depth === 0) {
          const tr = cur.trim();
          if (tr) list.push(tr);
          cur = '';
        } else {
          cur += c;
        }
      }
      const tr = cur.trim();
      if (tr) list.push(tr);
      return list;
    }

    function expandSelector(sel) {
      const s = sel.trim();
      const match = s.match(/:((?:where|is))\(/);
      if (!match) return [s.replace(/\s+/g, ' ')];

      const startIdx = match.index;
      const pLen = match[0].length;
      let depth = 1;
      let endIdx = -1;
      for (let i = startIdx + pLen; i < s.length; i++) {
        if (s[i] === '(') depth++;
        else if (s[i] === ')') {
          depth--;
          if (depth === 0) {
            endIdx = i;
            break;
          }
        }
      }
      if (endIdx === -1) return [s.replace(/\s+/g, ' ')];

      const prefix = s.slice(0, startIdx);
      const inner = s.slice(startIdx + pLen, endIdx);
      const suffix = s.slice(endIdx + 1);

      const list = splitTopLevelCommas(inner);
      const results = [];
      for (const item of list) {
        const combined = prefix + item + suffix;
        results.push(...expandSelector(combined));
      }
      return results;
    }

    const topLevelParts = splitTopLevelCommas(selectorString);
    const expanded = topLevelParts.flatMap(part => expandSelector(part));
    return Array.from(new Set(expanded.map(x => x.replace(/\s+/g, ' ').trim()))).filter(Boolean);
  }

  _addProperty(selector, atRuleScope, prop, value, important) {
    if (!this.selectorMap.has(selector)) {
      this.selectorMap.set(selector, new Map());
    }

    const scopeMap = this.selectorMap.get(selector);
    if (!scopeMap.has(atRuleScope)) {
      scopeMap.set(atRuleScope, new Map());
    }

    const propMap = scopeMap.get(atRuleScope);
    propMap.set(prop, { value, important });
  }

  /**
   * Reconstruct all components into unified rule blocks in wwwroot/css/components/
   */
  emitComponents() {
    fs.mkdirSync(this.componentsDir, { recursive: true });

    // Group selectors by component file
    // Map: filename -> Map<selector, Map<atRuleScope, Map<prop, { value, important }>>>
    const fileGroups = new Map();

    for (const [selector, scopeMap] of this.selectorMap.entries()) {
      const targetFile = resolveComponentCategory(selector);
      if (!fileGroups.has(targetFile)) {
        fileGroups.set(targetFile, new Map());
      }
      fileGroups.get(targetFile).set(selector, scopeMap);
    }

    let totalComponentFiles = 0;
    let totalClasses = 0;
    const generatedFiles = [];

    const sortedFiles = Array.from(fileGroups.keys()).sort((a, b) => {
      if (a === 'base.css') return -1;
      if (b === 'base.css') return 1;
      return a.localeCompare(b);
    });

    for (const filename of sortedFiles) {
      const selMap = fileGroups.get(filename);
      const fullPath = path.join(this.componentsDir, filename);

      let content = `/* stylelint-disable selector-class-pattern */\n`;
      content += `/* ==========================================================================\n`;
      content += `   Socratic Component Stylesheet: ${filename}\n`;
      content += `   Deconsolidated Component Architecture (All styles per class unified)\n`;
      content += `   ========================================================================== */\n\n`;

      const sortedSelectors = Array.from(selMap.keys()).sort((a, b) => a.localeCompare(b));

      // 1. Base (non-at-rule) scopes first
      for (const selector of sortedSelectors) {
        const scopeMap = selMap.get(selector);
        if (scopeMap.has('')) {
          const propMap = scopeMap.get('');
          content += this._formatComponentRule(selector, propMap);
          totalClasses++;
        }
      }

      // 2. At-rules (@media, @container)
      const allScopes = new Set();
      for (const scopeMap of selMap.values()) {
        for (const s of scopeMap.keys()) {
          if (s !== '') allScopes.add(s);
        }
      }

      const sortedScopes = Array.from(allScopes).sort((a, b) => a.localeCompare(b));
      for (const scope of sortedScopes) {
        content += `${scope} {\n`;
        for (const selector of sortedSelectors) {
          const scopeMap = selMap.get(selector);
          if (scopeMap.has(scope)) {
            const propMap = scopeMap.get(scope);
            content += this._formatComponentRule(selector, propMap, '    ');
          }
        }
        const openBraces = (scope.match(/{/g) || []).length + 1;
        content += `${'}\n'.repeat(openBraces)}\n`;
      }

      if (filename === 'base.css' && this.keyframes.size > 0) {
        content += `/* ==========================================================================\n`;
        content += `   Keyframes & Motion Definitions\n`;
        content += `   ========================================================================== */\n\n`;
        const sortedKeyframes = Array.from(this.keyframes.keys()).sort((a, b) => a.localeCompare(b));
        for (const kfName of sortedKeyframes) {
          content += `${this.keyframes.get(kfName)}\n\n`;
        }
      }

      fs.writeFileSync(fullPath, content, 'utf8');
      totalComponentFiles++;
      generatedFiles.push(filename);
    }

    // Generate master components index
    let indexContent = `/* stylelint-disable selector-class-pattern */\n`;
    indexContent += `/* ==========================================================================\n`;
    indexContent += `   Socratic Master Component Stylesheet\n`;
    indexContent += `   Reconstructed Component Architecture\n`;
    indexContent += `   ========================================================================== */\n\n`;

    for (const f of generatedFiles) {
      indexContent += `@import "${f}";\n`;
    }

    fs.writeFileSync(path.join(this.componentsDir, 'components.css'), indexContent, 'utf8');

    return { totalComponentFiles, totalClasses, generatedFiles };
  }

  _formatComponentRule(selector, propMap, indent = '') {
    const sortedProps = Array.from(propMap.keys()).sort((a, b) => {
      const wa = getPropWeight(a);
      const wb = getPropWeight(b);
      if (wa !== wb) return wa - wb;
      return a.localeCompare(b);
    });

    let rule = `${indent}${selector} {\n`;
    for (const p of sortedProps) {
      const decl = propMap.get(p);
      rule += `${indent}    ${p}: ${decl.value}${decl.important ? ' !important' : ''};\n`;
    }
    rule += `${indent}}\n\n`;
    return rule;
  }

  /**
   * Optimize token files in wwwroot/css/tokens/ by gathering all declarations per selector (:root)
   */
  optimizeTokens() {
    if (!fs.existsSync(this.tokensDir)) return { optimizedFiles: 0 };

    function getAllTokens(dir, list = []) {
      const items = fs.readdirSync(dir, { withFileTypes: true });
      for (const it of items) {
        const full = path.join(dir, it.name);
        if (it.isDirectory()) {
          getAllTokens(full, list);
        } else if (it.isFile() && it.name.endsWith('.css')) {
          list.push(full);
        }
      }
      return list;
    }

    const tokenFiles = getAllTokens(this.tokensDir);
    let optimizedCount = 0;

    for (const file of tokenFiles) {
      const content = fs.readFileSync(file, 'utf8');
      const root = postcss.parse(content, { from: file });

      // Map: selector -> Map<scope, Map<prop, value>>
      const fileSelMap = new Map();

      root.walkRules(rule => {
        const atRuleStack = [];
        let currentParent = rule.parent;
        while (currentParent && currentParent.type === 'atrule') {
          atRuleStack.unshift(`@${currentParent.name} ${currentParent.params.trim()}`);
          currentParent = currentParent.parent;
        }
        const atRuleScope = atRuleStack.join(' {\n    ');

        const sel = rule.selector.trim();
        if (!fileSelMap.has(sel)) {
          fileSelMap.set(sel, new Map());
        }
        const scopeMap = fileSelMap.get(sel);
        if (!scopeMap.has(atRuleScope)) {
          scopeMap.set(atRuleScope, new Map());
        }
        const propMap = scopeMap.get(atRuleScope);

        rule.walkDecls(decl => {
          propMap.set(decl.prop.trim(), decl.value.trim());
        });
      });

      if (fileSelMap.size === 0) continue;

      const relPath = path.relative(this.tokensDir, file).replace(/\\/g, '/');
      let newContent = `/* stylelint-disable selector-class-pattern */\n`;
      newContent += `/* ==========================================================================\n`;
      newContent += `   Socratic Design Tokens: tokens/${relPath}\n`;
      newContent += `   Optimized Unified Scopes\n`;
      newContent += `   ========================================================================== */\n\n`;

      const sortedSelectors = Array.from(fileSelMap.keys()).sort((a, b) => a.localeCompare(b));

      // 1. Top-level
      for (const sel of sortedSelectors) {
        const scopeMap = fileSelMap.get(sel);
        if (scopeMap.has('')) {
          const propMap = scopeMap.get('');
          newContent += `${sel} {\n`;
          const sortedProps = Array.from(propMap.keys()).sort((a, b) => a.localeCompare(b));
          for (const p of sortedProps) {
            newContent += `    ${p}: ${propMap.get(p)};\n`;
          }
          newContent += `}\n\n`;
        }
      }

      // 2. Scoped at-rules
      const allScopes = new Set();
      for (const scopeMap of fileSelMap.values()) {
        for (const s of scopeMap.keys()) {
          if (s !== '') allScopes.add(s);
        }
      }

      const sortedScopes = Array.from(allScopes).sort((a, b) => a.localeCompare(b));
      for (const scope of sortedScopes) {
        newContent += `${scope} {\n`;
        for (const sel of sortedSelectors) {
          const scopeMap = fileSelMap.get(sel);
          if (scopeMap.has(scope)) {
            const propMap = scopeMap.get(scope);
            newContent += `    ${sel} {\n`;
            const sortedProps = Array.from(propMap.keys()).sort((a, b) => a.localeCompare(b));
            for (const p of sortedProps) {
              newContent += `        ${p}: ${propMap.get(p)};\n`;
            }
            newContent += `    }\n`;
          }
        }
        const openBraces = (scope.match(/{/g) || []).length + 1;
        newContent += `${'}\n'.repeat(openBraces)}\n`;
      }

      fs.writeFileSync(file, newContent, 'utf8');
      optimizedCount++;
    }

    return { optimizedFiles: optimizedCount };
  }
}
