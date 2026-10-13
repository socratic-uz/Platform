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

    // Skip hidden elements and closed dialogs
    if (el.closest('dialog:not([open]), md-dialog:not([open]), [hidden]')) {
      return false;
    }

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

  function getOverlap(boxA, boxB) {
    const [ax, ay, aw, ah] = boxA;
    const [bx, by, bw, bh] = boxB;

    const overlapLeft = Math.max(ax, bx);
    const overlapRight = Math.min(ax + aw, bx + bw);
    const overlapTop = Math.max(ay, by);
    const overlapBottom = Math.min(ay + ah, by + bh);

    const overlapW = overlapRight - overlapLeft;
    const overlapH = overlapBottom - overlapTop;

    if (overlapW > 1 && overlapH > 1) {
      return {
        w: overlapW,
        h: overlapH,
        area: overlapW * overlapH
      };
    }
    return null;
  }

  function isInteractiveDescriptor(descriptor) {
    const tag = descriptor.split('.')[0].split('#')[0];
    const nonInteractiveMd = ['md-icon', 'md-divider', 'md-elevation', 'md-ripple', 'md-item', 'md-chip-set', 'md-linear-progress', 'md-circular-progress', 'md-tabs', 'md-segmented-button-set'];
    return (['button', 'a', 'input', 'select', 'textarea'].includes(tag) || (tag.startsWith('md-') && !nonInteractiveMd.includes(tag)));
  }

  function isOverlayNode(node) {
    if (!node.el || !(node.el instanceof Element)) return false;
    const desc = node.descriptor.toLowerCase();
    const classList = Array.from(node.el.classList || []).map(c => c.toLowerCase());
    const overlayTokens = [
      'badge', 'scrim', 'backdrop', 'overlay', 'tooltip', 'dialog',
      'menu', 'popup', 'modal', 'fab', 'glow', 'orb', 'ripple',
      'elevation', 'indicator', 'tab-bar', 'map-panel-toggle-tab', 'placeholder'
    ];
    if (overlayTokens.some(token => desc.includes(token) || classList.some(c => c.includes(token)))) {
      return true;
    }
    const style = window.getComputedStyle(node.el);
    if (style.pointerEvents === 'none') {
      return true;
    }
    return false;
  }

  function isAccidentalCollision(nodeA, nodeB) {
    if (!nodeA.el || !nodeB.el) return false;
    const styleA = window.getComputedStyle(nodeA.el);
    const styleB = window.getComputedStyle(nodeB.el);

    if (styleA.pointerEvents === 'none' || styleB.pointerEvents === 'none') {
      return false;
    }

    const isInteractA = isInteractiveDescriptor(nodeA.descriptor);
    const isInteractB = isInteractiveDescriptor(nodeB.descriptor);

    // Two interactive elements overlapping each other is ALWAYS an unintended collision
    if (isInteractA && isInteractB) {
      return true;
    }

    // If either element is an intentional overlay / decoration
    if (isOverlayNode(nodeA) || isOverlayNode(nodeB)) {
      return false;
    }

    // If both elements are in-flow (not absolute/fixed), overlap is a collision
    const isPosA = styleA.position === 'absolute' || styleA.position === 'fixed';
    const isPosB = styleB.position === 'absolute' || styleB.position === 'fixed';
    if (!isPosA && !isPosB) {
      return true;
    }

    return false;
  }

  function collectNodeGeometry(el, parentRect, parentEl, depth = 0) {
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

    // Detect silent text truncation / clipping in text elements with hidden overflow
    let textClip = null;
    const tag = el.tagName.toLowerCase();
    const isTextElement = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'span', 'button', 'a', 'label'].includes(tag);
    if (isTextElement && el.childElementCount <= 2) {
      const scrollW = Math.round(el.scrollWidth);
      const clientW = Math.round(el.clientWidth);
      const scrollH = Math.round(el.scrollHeight);
      const clientH = Math.round(el.clientHeight);
      const computed = window.getComputedStyle(el);

      const hasHorizontalClip = (computed.overflowX === 'hidden' || computed.overflow === 'hidden') && (scrollW > clientW + 2) && computed.textOverflow !== 'ellipsis';
      const hasVerticalClip = (computed.overflowY === 'hidden' || computed.overflow === 'hidden') && (scrollH > clientH + 2);

      if (hasHorizontalClip || hasVerticalClip) {
        textClip = {
          x: hasHorizontalClip ? (scrollW - clientW) : 0,
          y: hasVerticalClip ? (scrollH - clientH) : 0
        };
      }
    }

    const isInteractive = isInteractiveDescriptor(descriptor);
    const isChip = descriptor.includes('chip');
    const minTargetW = isChip ? 32 : 44;
    const minTargetH = isChip ? 32 : 44;
    const isTouchSmall = isInteractive && (width < minTargetW || height < minTargetH);

    // Filter meaningful children (treat SVG as a replaced element without traversing internal vector shapes)
    const isSvg = el.tagName.toLowerCase() === 'svg';
    const childNodes = isSvg ? [] : Array.from(el.children).filter(isMeaningfulNode);

    // Semantic & Nesting diagnostics
    const computed = window.getComputedStyle(el);
    const isGrid = computed.display === 'grid';
    const hasBorder = computed.borderWidth !== '0px' && computed.borderStyle !== 'none';
    const hasBg = computed.backgroundColor && computed.backgroundColor !== 'rgba(0, 0, 0, 0)' && computed.backgroundColor !== 'transparent';
    const hasSpecialRole = el.hasAttribute('role') || el.id || el.classList.contains('m3-card') || el.classList.contains('m3-surface');
    const isRedundantWrapper = !isGrid && !hasBorder && !hasBg && !hasSpecialRole && childNodes.length === 1 && childNodes[0].tagName.toLowerCase() === 'div';
    const isDeepNesting = depth >= 10;

    // Non-semantic button check
    const isFakeButton = ['div', 'span', 'p'].includes(tag) &&
      (computed.cursor === 'pointer' || Array.from(el.classList).some(c => c.endsWith('-btn') || c.endsWith('-button'))) &&
      !el.closest('button, a, summary, [role="button"], [role="link"], md-filled-card, md-elevated-card, md-outlined-card, md-filled-button, md-outlined-button, md-text-button, md-elevated-button, md-tonal-button, md-filter-chip, md-input-chip, md-assist-chip');

    const children = childNodes.map(child => collectNodeGeometry(child, rect, el, depth + 1));

    // Detect sibling collisions and touch proximity among direct children
    const childCount = children.length;
    for (let i = 0; i < childCount; i++) {
      for (let j = i + 1; j < childCount; j++) {
        const childA = children[i];
        const childB = children[j];
        const overlap = getOverlap(childA.box, childB.box);
        if (overlap && isAccidentalCollision(childA, childB)) {
          childA.collisions.push({ sibling: childB.descriptor, w: overlap.w, h: overlap.h, area: overlap.area });
          childB.collisions.push({ sibling: childA.descriptor, w: overlap.w, h: overlap.h, area: overlap.area });
        } else if (!overlap) {
          const isIntA = isInteractiveDescriptor(childA.descriptor);
          const isIntB = isInteractiveDescriptor(childB.descriptor);
          if (isIntA && isIntB) {
            const [ax, ay, aw, ah] = childA.box;
            const [bx, by, bw, bh] = childB.box;
            const gapX = Math.max(0, bx - (ax + aw), ax - (bx + bw));
            const gapY = Math.max(0, by - (ay + ah), ay - (by + bh));
            const minGap = Math.max(gapX, gapY);
            if (minGap < 8) {
              childB.touchProximities.push({ sibling: childA.descriptor, gap: minGap });
            }
          }
        }
      }
    }

    return {
      el,
      descriptor,
      depth,
      box: [x, y, width, height],
      insets: [left, top, right, bottom],
      isOverflow,
      isOverflowX,
      isOverflowY,
      isTouchSmall,
      isRedundantWrapper,
      isDeepNesting,
      isFakeButton,
      textClip,
      touchProximities: [],
      collisions: [],
      children
    };
  }

  function formatAsciiTree(node, prefix = '', isLast = true, isRoot = true) {
    const [x, y, w, h] = node.box;
    const [l, t, r, b] = node.insets;
    const overflowMarker = node.isOverflow ? ` <-- ❌ [overflow: r=${r}px, b=${b}px]` : '';
    const collisionMarker = (node.collisions && node.collisions.length > 0)
      ? ` <-- ⚠️ [collision with ${node.collisions.map(c => `${c.sibling} (${c.w}x${c.h}px)`).join(', ')}]`
      : '';
    const touchSmallMarker = node.isTouchSmall
      ? ` <-- 👆 [touch-small: ${w}x${h}px < 44px]`
      : '';
    const touchProxMarker = (node.touchProximities && node.touchProximities.length > 0)
      ? ` <-- 👆 [touch-gap with ${node.touchProximities.map(p => `${p.sibling}: ${p.gap}px`).join(', ')} < 8px]`
      : '';
    const textClipMarker = node.textClip
      ? ` <-- ✂️ [text-clipped: ${node.textClip.x ? `+${node.textClip.x}px horiz` : ''}${node.textClip.y ? `+${node.textClip.y}px vert` : ''}]`
      : '';
    const redundantMarker = node.isRedundantWrapper ? ' <-- 📦 [redundant-wrapper]' : '';
    const deepNestingMarker = node.isDeepNesting ? ` <-- 🌊 [deep-nesting: depth=${node.depth}]` : '';
    const fakeButtonMarker = node.isFakeButton ? ' <-- ♿ [non-semantic-action: div/span should be <button>]' : '';

    const allMarkers = `${overflowMarker}${collisionMarker}${touchSmallMarker}${touchProxMarker}${textClipMarker}${redundantMarker}${deepNestingMarker}${fakeButtonMarker}`;
    
    let line = '';
    if (isRoot) {
      line = `${node.descriptor} [${x},${y},${w},${h}] [${l},${t},${r},${b}]${allMarkers}\n`;
    } else {
      const branch = isLast ? '└── ' : '├── ';
      line = `${prefix}${branch}${node.descriptor} [${x},${y},${w},${h}] [${l},${t},${r},${b}]${allMarkers}\n`;
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

    if (node.isRedundantWrapper) {
      violations.push({
        type: 'RedundantWrapper',
        target: node.descriptor,
        path: currentPath,
        box: [x, y, w, h],
        insets: [l, t, r, b],
        message: `Redundant single-child <div> wrapper detected with no distinct layout or styles.`
      });
    }

    if (node.isDeepNesting && node.depth >= 12) {
      violations.push({
        type: 'DeepNesting',
        target: node.descriptor,
        path: currentPath,
        box: [x, y, w, h],
        insets: [l, t, r, b],
        message: `Excessive DOM nesting depth: level ${node.depth} exceeds threshold 12.`
      });
    }

    if (node.isFakeButton) {
      violations.push({
        type: 'NonSemanticAction',
        target: node.descriptor,
        path: currentPath,
        box: [x, y, w, h],
        insets: [l, t, r, b],
        message: `Interactive click target implemented as <${node.descriptor.split('.')[0]}> instead of <button> or <a>.`
      });
    }

    // Touch target validation for interactive elements
    const tag = node.descriptor.split('.')[0].split('#')[0];
    const isInteractive = isInteractiveDescriptor(node.descriptor);
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

    // Sibling Collisions among direct children
    for (let i = 0; i < node.children.length; i++) {
      for (let j = i + 1; j < node.children.length; j++) {
        const childA = node.children[i];
        const childB = node.children[j];
        const overlap = getOverlap(childA.box, childB.box);
        if (overlap && isAccidentalCollision(childA, childB)) {
          violations.push({
            type: 'SiblingCollision',
            target: childB.descriptor,
            sibling: childA.descriptor,
            parent: node.descriptor,
            path: `${currentPath} > (${childA.descriptor} ⚡ ${childB.descriptor})`,
            box: childB.box,
            overlap: [overlap.w, overlap.h],
            area: overlap.area,
            message: `Siblings '${childA.descriptor}' and '${childB.descriptor}' collide by ${overlap.w}x${overlap.h}px (${overlap.area}px²)`
          });
        }
      }
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

      if (!document.querySelector('main')) {
        violations.push({
          type: 'MissingLandmark',
          target: 'body',
          path: 'body',
          message: 'Document is missing a <main> landmark element.'
        });
      }

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
