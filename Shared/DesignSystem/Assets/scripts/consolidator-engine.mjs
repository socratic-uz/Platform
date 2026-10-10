/**
 * Socratic Platform - AST Single-Declaration Consolidator Engine
 * Parses CSS via PostCSS AST, classifies properties across 41 W3C modules,
 * and guarantees every property-value declaration occurs EXACTLY ONCE per scope.
 */

import fs from 'node:fs';
import path from 'node:path';
import postcss from 'postcss';
import { resolveModuleForProperty } from './w3c-taxonomy.mjs';

export class AstConsolidator {
  constructor(options = {}) {
    this.outputDir = options.outputDir;
    // Map: moduleRelPath -> Map<atRuleScope, Map<declString, Set<selector>>>
    this.modules = new Map();
    // Keyframes storage: keyframeName -> raw CSS string
    this.keyframes = new Map();
  }

  /**
   * Process CSS content and distribute declarations into W3C modules
   */
  processCss(cssContent, sourceFileName = 'input.css') {
    const root = postcss.parse(cssContent, { from: sourceFileName });

    root.walk(node => {
      // 1. Keyframes are preserved as atomic animation units
      if (node.type === 'atrule' && (node.name === 'keyframes' || node.name === '-webkit-keyframes')) {
        const keyframeCss = node.toString().trim();
        this.keyframes.set(node.params.trim(), keyframeCss);
        return;
      }

      // 2. Rules containing property declarations
      if (node.type === 'rule') {
        // Skip rules inside keyframes
        if (node.parent && node.parent.type === 'atrule' && node.parent.name.includes('keyframes')) {
          return;
        }

        // Determine Nested At-Rule Scope (@media, @container, @supports)
        const atRuleStack = [];
        let currentParent = node.parent;
        while (currentParent && currentParent.type === 'atrule') {
          atRuleStack.unshift(`@${currentParent.name} ${currentParent.params.trim()}`);
          currentParent = currentParent.parent;
        }
        const atRuleScope = atRuleStack.join(' {\n    ');

        // Extract clean selectors
        const selectors = node.selectors
          ? node.selectors.map(s => s.trim().replace(/\s+/g, ' ')).filter(Boolean)
          : [];

        if (selectors.length === 0) return;

        node.walkDecls(decl => {
          const prop = decl.prop.trim().toLowerCase();
          const value = decl.value.trim();

          const targetModule = resolveModuleForProperty(prop);
          if (!targetModule) {
            return;
          }

          const declString = `${prop}: ${value}${decl.important ? ' !important' : ''}`;
          this._addDeclaration(targetModule, atRuleScope, declString, selectors);
        });
      }
    });
  }

  _addDeclaration(modulePath, atRuleScope, declString, selectors) {
    if (!this.modules.has(modulePath)) {
      this.modules.set(modulePath, new Map());
    }

    const scopeMap = this.modules.get(modulePath);
    if (!scopeMap.has(atRuleScope)) {
      scopeMap.set(atRuleScope, new Map());
    }

    const declMap = scopeMap.get(atRuleScope);
    if (!declMap.has(declString)) {
      declMap.set(declString, new Set());
    }

    const selectorSet = declMap.get(declString);
    for (const sel of selectors) {
      selectorSet.add(sel);
    }
  }

  /**
   * Render all consolidated CSS files to disk
   */
  emitFiles() {
    let totalFiles = 0;
    let totalDeclarations = 0;

    for (const [moduleRelPath, scopeMap] of this.modules.entries()) {
      const fullPath = path.join(this.outputDir, moduleRelPath);
      fs.mkdirSync(path.dirname(fullPath), { recursive: true });

      let content = `/* stylelint-disable selector-class-pattern */\n`;
      content += `/* ==========================================================================\n`;
      content += `   W3C CSS Module: ${moduleRelPath}\n`;
      content += `   Consolidated Single-Declaration Rules (:where() Zero Specificity)\n`;
      content += `   ========================================================================== */\n\n`;

      // 1. Top-level (non-at-rule) declarations first
      if (scopeMap.has('')) {
        const topLevelDecls = scopeMap.get('');
        // Sort declarations for deterministic reproducible builds
        const sortedDeclKeys = Array.from(topLevelDecls.keys()).sort((a, b) => a.localeCompare(b));
        for (const declString of sortedDeclKeys) {
          const selectorSet = topLevelDecls.get(declString);
          content += this._formatRule(selectorSet, declString, '', moduleRelPath);
          totalDeclarations++;
        }
      }

      // 2. Scoped at-rules (@media, @container)
      const sortedScopes = Array.from(scopeMap.keys()).filter(s => s !== '').sort((a, b) => a.localeCompare(b));
      for (const scope of sortedScopes) {
        const declMap = scopeMap.get(scope);
        content += `${scope} {\n`;
        const sortedDeclKeys = Array.from(declMap.keys()).sort((a, b) => a.localeCompare(b));
        for (const declString of sortedDeclKeys) {
          const selectorSet = declMap.get(declString);
          content += this._formatRule(selectorSet, declString, '    ', moduleRelPath);
          totalDeclarations++;
        }
        content += `}\n\n`;
      }

      // Special case: if this is motion/animations.css, append gathered keyframes
      if (moduleRelPath === 'motion/animations.css' && this.keyframes.size > 0) {
        content += `/* ==========================================================================\n`;
        content += `   Atomic Keyframes Animations\n`;
        content += `   ========================================================================== */\n\n`;
        const sortedKeyframes = Array.from(this.keyframes.keys()).sort((a, b) => a.localeCompare(b));
        for (const kfName of sortedKeyframes) {
          content += `${this.keyframes.get(kfName)}\n\n`;
        }
      }

      fs.writeFileSync(fullPath, content, 'utf8');
      totalFiles++;
    }

    return { totalFiles, totalDeclarations };
  }

  _formatRule(selectorSet, declString, indent = '', moduleRelPath = '') {
    const sortedSelectors = Array.from(selectorSet).sort((a, b) => a.localeCompare(b));

    // Special case: gradient-text.css MUST NOT use :where() to preserve specificity over background shorthand
    if (moduleRelPath === 'color/gradient-text.css') {
      return `${indent}${sortedSelectors.join(`,\n${indent}`)} {\n${indent}    ${declString};\n${indent}}\n\n`;
    }

    if (sortedSelectors.length === 1) {
      return `${indent}${sortedSelectors[0]} {\n${indent}    ${declString};\n${indent}}\n\n`;
    }

    return `${indent}:where(\n${indent}    ${sortedSelectors.join(`,\n${indent}    `)}\n${indent}) {\n${indent}    ${declString};\n${indent}}\n\n`;
  }
}
