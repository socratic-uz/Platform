/**
 * Socratic Preview Collector — In-Browser Layout Blueprint Generator
 * 
 * Extracts deterministic hierarchical layout geometry without screenshots.
 * Formula:
 *   Тег [x, y, width, height] [l, t, r, b]
 * 
 * Rules:
 *   - Coordinates [x, y, width, height] are relative to the parent coordinate origin (0,0).
 *   - Insets [l, t, r, b] represent distances from parent boundaries to element boundaries.
 *   - Negative insets indicate overflow/clipping outside the parent container.
 */

(function () {
  'use strict';

  function getElementDescriptor(el) {
    const tag = el.tagName.toLowerCase();
    
    // Check if it's an M3 Web Component or custom component
    if (tag.startsWith('md-')) {
      return tag;
    }

    // Try identifying semantic Blazor/M3 classes
    const classList = Array.from(el.classList || []);
    const m3Class = classList.find(c => c.startsWith('m3-') || c.startsWith('catalog-') || c.includes('card') || c.includes('btn') || c.includes('sidebar'));
    
    if (m3Class) {
      return `${tag}.${m3Class}`;
    }

    if (el.id) {
      return `${tag}#${el.id}`;
    }

    return tag;
  }

  function isMeaningfulNode(el) {
    if (!el || el.nodeType !== Node.ELEMENT_NODE) return false;
    
    const tag = el.tagName.toLowerCase();
    if (['script', 'style', 'link', 'meta', 'noscript', 'template'].includes(tag)) {
      return false;
    }

    // Skip purely decorative backdrop ambient glow meshes
    if (el.classList.contains('cursor-glow-orb') ||
        el.classList.contains('landing-bg-canvas') ||
        el.closest('.landing-bg-canvas') ||
        el.classList.contains('section-ambient-glow') ||
        el.closest('.section-ambient-glow')) {
      return false;
    }

    // Skip hidden elements
    const style = window.getComputedStyle(el);
    if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
      return false;
    }

    // Skip zero-dimension non-containers
    const rect = el.getBoundingClientRect();
    if (rect.width <= 0 && rect.height <= 0) {
      return false;
    }

    return true;
  }

  function collectNodeGeometry(el, parentRect, parentEl) {
    const rect = el.getBoundingClientRect();
    const pRect = parentRect || {
      left: 0,
      top: 0,
      width: window.innerWidth,
      height: window.innerHeight
    };

    // Calculate relative coordinates to parent origin
    const x = Math.round(rect.left - pRect.left);
    const y = Math.round(rect.top - pRect.top);
    const width = Math.round(rect.width);
    const height = Math.round(rect.height);

    // Calculate insets from parent bounds
    const left = x;
    const top = y;
    const right = Math.round(pRect.width - (x + width));
    const bottom = Math.round(pRect.height - (y + height));

    // Determine legitimate scroll containers (Localized Scroll Invariant)
    let parentAllowsXScroll = false;
    let parentAllowsYScroll = false;
    if (parentEl && parentEl instanceof Element) {
      const pStyle = window.getComputedStyle(parentEl);
      parentAllowsXScroll = pStyle.overflowX === 'auto' || pStyle.overflowX === 'scroll';
      parentAllowsYScroll = pStyle.overflowY === 'auto' || pStyle.overflowY === 'scroll';
    }

    const isRootDocument = el.tagName.toLowerCase() === 'body' || el.tagName.toLowerCase() === 'html';
    const descriptor = getElementDescriptor(el);
    const isFloatingTab = el.classList.contains('map-panel-toggle-tab');
    const isOverflowX = isFloatingTab ? false : (right < -1 && !parentAllowsXScroll);
    const isOverflowY = isRootDocument ? false : (bottom < -1 && !parentAllowsYScroll);
    const isOverflow = isOverflowX || isOverflowY;

    // Filter meaningful children (treat SVG as a replaced element without traversing internal vector shapes)
    const isSvg = el.tagName.toLowerCase() === 'svg';
    const childNodes = isSvg ? [] : Array.from(el.children).filter(isMeaningfulNode);
    const children = childNodes.map(child => collectNodeGeometry(child, rect, el));

    return {
      el,
      descriptor,
      box: [x, y, width, height],
      insets: [left, top, right, bottom],
      isOverflow,
      isOverflowX,
      isOverflowY,
      children
    };
  }

  function formatAsciiTree(node, prefix = '', isLast = true, isRoot = true) {
    const [x, y, w, h] = node.box;
    const [l, t, r, b] = node.insets;
    const overflowMarker = node.isOverflow ? ` <-- ❌ [overflow: r=${r}px, b=${b}px]` : '';
    
    let line = '';
    if (isRoot) {
      line = `${node.descriptor} [${x},${y},${w},${h}] [${l},${t},${r},${b}]${overflowMarker}\n`;
    } else {
      const branch = isLast ? '└── ' : '├── ';
      line = `${prefix}${branch}${node.descriptor} [${x},${y},${w},${h}] [${l},${t},${r},${b}]${overflowMarker}\n`;
    }

    const nextPrefix = isRoot ? '    ' : prefix + (isLast ? '    ' : '│   ');
    const childCount = node.children.length;

    for (let i = 0; i < childCount; i++) {
      const childIsLast = i === childCount - 1;
      line += formatAsciiTree(node.children[i], nextPrefix, childIsLast, false);
    }

    return line;
  }

  function findViolations(node, path = '', violations = []) {
    const currentPath = path ? `${path} > ${node.descriptor}` : node.descriptor;
    const [x, y, w, h] = node.box;
    const [l, t, r, b] = node.insets;

    if (node.isOverflow) {
      violations.push({
        type: 'BoundaryOverflow',
        target: node.descriptor,
        path: currentPath,
        box: [x, y, w, h],
        insets: [l, t, r, b],
        overflowX: node.isOverflowX ? Math.abs(r) : 0,
        overflowY: node.isOverflowY ? Math.abs(b) : 0,
        message: `Element overflows parent container by ${node.isOverflowX ? Math.abs(r) + 'px horizontally' : ''} ${node.isOverflowY ? Math.abs(b) + 'px vertically' : ''}`
      });
    }

    // Touch target validation for interactive elements
    const tag = node.descriptor.split('.')[0].split('#')[0];
    const nonInteractiveMd = ['md-icon', 'md-divider', 'md-elevation', 'md-ripple', 'md-item', 'md-chip-set', 'md-linear-progress', 'md-circular-progress', 'md-tabs', 'md-segmented-button-set'];
    const isInteractive = (['button', 'a', 'input', 'select', 'textarea'].includes(tag) || (tag.startsWith('md-') && !nonInteractiveMd.includes(tag)));
    const isChip = tag.includes('chip');
    const minTargetW = isChip ? 32 : 44;
    const minTargetH = isChip ? 32 : 44;
    if (isInteractive && (w < minTargetW || h < minTargetH)) {
      violations.push({
        type: 'TouchTargetSmall',
        target: node.descriptor,
        path: currentPath,
        box: [x, y, w, h],
        insets: [l, t, r, b],
        message: `Interactive target is smaller than 44x44px (${w}x${h}px)`
      });
    }

    for (const child of node.children) {
      findViolations(child, currentPath, violations);
    }

    return violations;
  }

  // Public API attached to window
  window.SocraticLayoutAnalyzer = {
    captureTree(rootSelector = 'body') {
      const root = document.querySelector(rootSelector);
      if (!root) throw new Error(`Root selector '${rootSelector}' not found.`);
      return collectNodeGeometry(root, null);
    },

    exportLayout(rootSelector = 'body') {
      const tree = this.captureTree(rootSelector);
      return formatAsciiTree(tree);
    },

    audit(rootSelector = 'body') {
      const tree = this.captureTree(rootSelector);
      const textLayout = formatAsciiTree(tree);
      const violations = findViolations(tree);
      return {
        tree,
        layout: textLayout,
        violationsCount: violations.length,
        violations
      };
    }
  };

  console.log('✅ Socratic Preview Collector initialized. Use window.SocraticLayoutAnalyzer.exportLayout()');
})();
