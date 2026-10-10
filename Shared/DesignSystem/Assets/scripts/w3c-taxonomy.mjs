/**
 * Socratic Platform - Comprehensive W3C CSS Module Taxonomy
 * Comprehensive map of ~300 standard W3C CSS properties & logical properties
 * mapped to Socratic's 41 W3C modular stylesheets.
 */

export const PROPERTY_TO_W3C_MODULE = new Map([
  // ──────────────────────────────────────────────────────────────────────────
  // 1. Box Model: Sizing (box-model/sizing.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['width', 'box-model/sizing.css'],
  ['min-width', 'box-model/sizing.css'],
  ['max-width', 'box-model/sizing.css'],
  ['height', 'box-model/sizing.css'],
  ['min-height', 'box-model/sizing.css'],
  ['max-height', 'box-model/sizing.css'],
  ['inline-size', 'box-model/sizing.css'],
  ['min-inline-size', 'box-model/sizing.css'],
  ['max-inline-size', 'box-model/sizing.css'],
  ['block-size', 'box-model/sizing.css'],
  ['min-block-size', 'box-model/sizing.css'],
  ['max-block-size', 'box-model/sizing.css'],
  ['box-sizing', 'box-model/sizing.css'],
  ['aspect-ratio', 'box-model/sizing.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 2. Box Model: Margin (box-model/margin.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['margin', 'box-model/margin.css'],
  ['margin-top', 'box-model/margin.css'],
  ['margin-right', 'box-model/margin.css'],
  ['margin-bottom', 'box-model/margin.css'],
  ['margin-left', 'box-model/margin.css'],
  ['margin-inline', 'box-model/margin.css'],
  ['margin-inline-start', 'box-model/margin.css'],
  ['margin-inline-end', 'box-model/margin.css'],
  ['margin-block', 'box-model/margin.css'],
  ['margin-block-start', 'box-model/margin.css'],
  ['margin-block-end', 'box-model/margin.css'],
  ['margin-trim', 'box-model/margin.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 3. Box Model: Padding (box-model/padding.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['padding', 'box-model/padding.css'],
  ['padding-top', 'box-model/padding.css'],
  ['padding-right', 'box-model/padding.css'],
  ['padding-bottom', 'box-model/padding.css'],
  ['padding-left', 'box-model/padding.css'],
  ['padding-inline', 'box-model/padding.css'],
  ['padding-inline-start', 'box-model/padding.css'],
  ['padding-inline-end', 'box-model/padding.css'],
  ['padding-block', 'box-model/padding.css'],
  ['padding-block-start', 'box-model/padding.css'],
  ['padding-block-end', 'box-model/padding.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 4. Box Model: Border (box-model/border.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['border', 'box-model/border.css'],
  ['border-width', 'box-model/border.css'],
  ['border-style', 'box-model/border.css'],
  ['border-color', 'box-model/border.css'],
  ['border-radius', 'box-model/border.css'],
  ['border-top-left-radius', 'box-model/border.css'],
  ['border-top-right-radius', 'box-model/border.css'],
  ['border-bottom-left-radius', 'box-model/border.css'],
  ['border-bottom-right-radius', 'box-model/border.css'],
  ['border-start-start-radius', 'box-model/border.css'],
  ['border-start-end-radius', 'box-model/border.css'],
  ['border-end-start-radius', 'box-model/border.css'],
  ['border-end-end-radius', 'box-model/border.css'],
  ['border-top', 'box-model/border.css'],
  ['border-top-width', 'box-model/border.css'],
  ['border-top-style', 'box-model/border.css'],
  ['border-top-color', 'box-model/border.css'],
  ['border-right', 'box-model/border.css'],
  ['border-right-width', 'box-model/border.css'],
  ['border-right-style', 'box-model/border.css'],
  ['border-right-color', 'box-model/border.css'],
  ['border-bottom', 'box-model/border.css'],
  ['border-bottom-width', 'box-model/border.css'],
  ['border-bottom-style', 'box-model/border.css'],
  ['border-bottom-color', 'box-model/border.css'],
  ['border-left', 'box-model/border.css'],
  ['border-left-width', 'box-model/border.css'],
  ['border-left-style', 'box-model/border.css'],
  ['border-left-color', 'box-model/border.css'],
  ['border-inline', 'box-model/border.css'],
  ['border-inline-start', 'box-model/border.css'],
  ['border-inline-end', 'box-model/border.css'],
  ['border-inline-width', 'box-model/border.css'],
  ['border-inline-start-width', 'box-model/border.css'],
  ['border-inline-end-width', 'box-model/border.css'],
  ['border-inline-style', 'box-model/border.css'],
  ['border-inline-start-style', 'box-model/border.css'],
  ['border-inline-end-style', 'box-model/border.css'],
  ['border-inline-color', 'box-model/border.css'],
  ['border-inline-start-color', 'box-model/border.css'],
  ['border-inline-end-color', 'box-model/border.css'],
  ['border-block', 'box-model/border.css'],
  ['border-block-start', 'box-model/border.css'],
  ['border-block-end', 'box-model/border.css'],
  ['border-block-width', 'box-model/border.css'],
  ['border-block-start-width', 'box-model/border.css'],
  ['border-block-end-width', 'box-model/border.css'],
  ['border-block-style', 'box-model/border.css'],
  ['border-block-start-style', 'box-model/border.css'],
  ['border-block-end-style', 'box-model/border.css'],
  ['border-block-color', 'box-model/border.css'],
  ['border-block-start-color', 'box-model/border.css'],
  ['border-block-end-color', 'box-model/border.css'],
  ['border-image', 'box-model/border.css'],
  ['border-image-source', 'box-model/border.css'],
  ['border-image-slice', 'box-model/border.css'],
  ['border-image-width', 'box-model/border.css'],
  ['border-image-outset', 'box-model/border.css'],
  ['border-image-repeat', 'box-model/border.css'],
  ['border-collapse', 'box-model/border.css'],
  ['border-spacing', 'box-model/border.css'],
  ['box-decoration-break', 'box-model/border.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 5. Box Model: Outline (box-model/outline.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['outline', 'box-model/outline.css'],
  ['outline-width', 'box-model/outline.css'],
  ['outline-style', 'box-model/outline.css'],
  ['outline-color', 'box-model/outline.css'],
  ['outline-offset', 'box-model/outline.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 6. Layout: Display (layout/display.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['display', 'layout/display.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 7. Layout: Grid (layout/grid.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['grid', 'layout/grid.css'],
  ['grid-template', 'layout/grid.css'],
  ['grid-template-columns', 'layout/grid.css'],
  ['grid-template-rows', 'layout/grid.css'],
  ['grid-template-areas', 'layout/grid.css'],
  ['grid-auto-flow', 'layout/grid.css'],
  ['grid-auto-columns', 'layout/grid.css'],
  ['grid-auto-rows', 'layout/grid.css'],
  ['grid-column', 'layout/grid.css'],
  ['grid-column-start', 'layout/grid.css'],
  ['grid-column-end', 'layout/grid.css'],
  ['grid-row', 'layout/grid.css'],
  ['grid-row-start', 'layout/grid.css'],
  ['grid-row-end', 'layout/grid.css'],
  ['grid-area', 'layout/grid.css'],
  ['subgrid', 'layout/grid.css'],
  ['masonry-auto-flow', 'layout/grid.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 8. Layout: Alignment (layout/alignment.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['gap', 'layout/alignment.css'],
  ['row-gap', 'layout/alignment.css'],
  ['column-gap', 'layout/alignment.css'],
  ['justify-content', 'layout/alignment.css'],
  ['justify-items', 'layout/alignment.css'],
  ['justify-self', 'layout/alignment.css'],
  ['align-content', 'layout/alignment.css'],
  ['align-items', 'layout/alignment.css'],
  ['align-self', 'layout/alignment.css'],
  ['place-content', 'layout/alignment.css'],
  ['place-items', 'layout/alignment.css'],
  ['place-self', 'layout/alignment.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 9. Layout: Multi-column (layout/multi-column.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['columns', 'layout/multi-column.css'],
  ['column-count', 'layout/multi-column.css'],
  ['column-width', 'layout/multi-column.css'],
  ['column-rule', 'layout/multi-column.css'],
  ['column-rule-width', 'layout/multi-column.css'],
  ['column-rule-style', 'layout/multi-column.css'],
  ['column-rule-color', 'layout/multi-column.css'],
  ['column-fill', 'layout/multi-column.css'],
  ['column-span', 'layout/multi-column.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 10. Layout: Table (layout/table.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['table-layout', 'layout/table.css'],
  ['caption-side', 'layout/table.css'],
  ['empty-cells', 'layout/table.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 11. Positioning: Position (positioning/position.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['position', 'positioning/position.css'],
  ['top', 'positioning/position.css'],
  ['right', 'positioning/position.css'],
  ['bottom', 'positioning/position.css'],
  ['left', 'positioning/position.css'],
  ['inset', 'positioning/position.css'],
  ['inset-inline', 'positioning/position.css'],
  ['inset-inline-start', 'positioning/position.css'],
  ['inset-inline-end', 'positioning/position.css'],
  ['inset-block', 'positioning/position.css'],
  ['inset-block-start', 'positioning/position.css'],
  ['inset-block-end', 'positioning/position.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 12. Positioning: Stacking (positioning/stacking.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['z-index', 'positioning/stacking.css'],
  ['isolation', 'positioning/stacking.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 13. Positioning: Anchor (positioning/anchor.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['anchor-name', 'positioning/anchor.css'],
  ['position-anchor', 'positioning/anchor.css'],
  ['position-area', 'positioning/anchor.css'],
  ['position-try', 'positioning/anchor.css'],
  ['position-try-options', 'positioning/anchor.css'],
  ['position-try-order', 'positioning/anchor.css'],
  ['position-visibility', 'positioning/anchor.css'],
  ['anchor-scope', 'positioning/anchor.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 14. Overflow: Overflow (overflow/overflow.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['overflow', 'overflow/overflow.css'],
  ['overflow-x', 'overflow/overflow.css'],
  ['overflow-y', 'overflow/overflow.css'],
  ['overflow-block', 'overflow/overflow.css'],
  ['overflow-inline', 'overflow/overflow.css'],
  ['overflow-clip-margin', 'overflow/overflow.css'],
  ['overscroll-behavior', 'overflow/overflow.css'],
  ['overscroll-behavior-x', 'overflow/overflow.css'],
  ['overscroll-behavior-y', 'overflow/overflow.css'],
  ['overscroll-behavior-block', 'overflow/overflow.css'],
  ['overscroll-behavior-inline', 'overflow/overflow.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 15. Overflow: Scrolling (overflow/scrolling.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['scroll-behavior', 'overflow/scrolling.css'],
  ['scroll-margin', 'overflow/scrolling.css'],
  ['scroll-margin-top', 'overflow/scrolling.css'],
  ['scroll-margin-right', 'overflow/scrolling.css'],
  ['scroll-margin-bottom', 'overflow/scrolling.css'],
  ['scroll-margin-left', 'overflow/scrolling.css'],
  ['scroll-margin-inline', 'overflow/scrolling.css'],
  ['scroll-margin-inline-start', 'overflow/scrolling.css'],
  ['scroll-margin-inline-end', 'overflow/scrolling.css'],
  ['scroll-margin-block', 'overflow/scrolling.css'],
  ['scroll-margin-block-start', 'overflow/scrolling.css'],
  ['scroll-margin-block-end', 'overflow/scrolling.css'],
  ['scroll-padding', 'overflow/scrolling.css'],
  ['scroll-padding-top', 'overflow/scrolling.css'],
  ['scroll-padding-right', 'overflow/scrolling.css'],
  ['scroll-padding-bottom', 'overflow/scrolling.css'],
  ['scroll-padding-left', 'overflow/scrolling.css'],
  ['scroll-padding-inline', 'overflow/scrolling.css'],
  ['scroll-padding-inline-start', 'overflow/scrolling.css'],
  ['scroll-padding-inline-end', 'overflow/scrolling.css'],
  ['scroll-padding-block', 'overflow/scrolling.css'],
  ['scroll-padding-block-start', 'overflow/scrolling.css'],
  ['scroll-padding-block-end', 'overflow/scrolling.css'],
  ['scroll-snap-type', 'overflow/scrolling.css'],
  ['scroll-snap-align', 'overflow/scrolling.css'],
  ['scroll-snap-stop', 'overflow/scrolling.css'],
  ['scroll-timeline', 'overflow/scrolling.css'],
  ['scroll-timeline-name', 'overflow/scrolling.css'],
  ['scroll-timeline-axis', 'overflow/scrolling.css'],
  ['view-timeline', 'overflow/scrolling.css'],
  ['view-timeline-name', 'overflow/scrolling.css'],
  ['view-timeline-axis', 'overflow/scrolling.css'],
  ['view-timeline-inset', 'overflow/scrolling.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 16. Overflow: Scrollbars (overflow/scrollbars.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['scrollbar-width', 'overflow/scrollbars.css'],
  ['scrollbar-color', 'overflow/scrollbars.css'],
  ['scrollbar-gutter', 'overflow/scrollbars.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 17. Typography: Font (typography/font.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['font', 'typography/font.css'],
  ['font-family', 'typography/font.css'],
  ['font-size', 'typography/font.css'],
  ['font-size-adjust', 'typography/font.css'],
  ['font-weight', 'typography/font.css'],
  ['font-style', 'typography/font.css'],
  ['font-display', 'typography/font.css'],
  ['font-stretch', 'typography/font.css'],
  ['font-variant', 'typography/font.css'],
  ['font-variant-caps', 'typography/font.css'],
  ['font-variant-numeric', 'typography/font.css'],
  ['font-variant-alternates', 'typography/font.css'],
  ['font-variant-east-asian', 'typography/font.css'],
  ['font-variant-ligatures', 'typography/font.css'],
  ['font-variant-position', 'typography/font.css'],
  ['font-feature-settings', 'typography/font.css'],
  ['font-variation-settings', 'typography/font.css'],
  ['font-kerning', 'typography/font.css'],
  ['font-optical-sizing', 'typography/font.css'],
  ['font-palette', 'typography/font.css'],
  ['font-synthesis', 'typography/font.css'],
  ['-webkit-font-smoothing', 'typography/font.css'],
  ['-moz-osx-font-smoothing', 'typography/font.css'],
  ['line-height', 'typography/font.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 18. Typography: Text Layout (typography/text-layout.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['text-align', 'typography/text-layout.css'],
  ['text-align-last', 'typography/text-layout.css'],
  ['text-justify', 'typography/text-layout.css'],
  ['text-indent', 'typography/text-layout.css'],
  ['vertical-align', 'typography/text-layout.css'],
  ['letter-spacing', 'typography/text-layout.css'],
  ['word-spacing', 'typography/text-layout.css'],
  ['text-transform', 'typography/text-layout.css'],
  ['tab-size', 'typography/text-layout.css'],
  ['ruby-align', 'typography/text-layout.css'],
  ['ruby-position', 'typography/text-layout.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 19. Typography: Text Wrapping (typography/text-wrapping.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['white-space', 'typography/text-wrapping.css'],
  ['white-space-collapse', 'typography/text-wrapping.css'],
  ['text-wrap', 'typography/text-wrapping.css'],
  ['text-wrap-mode', 'typography/text-wrapping.css'],
  ['text-wrap-style', 'typography/text-wrapping.css'],
  ['word-break', 'typography/text-wrapping.css'],
  ['overflow-wrap', 'typography/text-wrapping.css'],
  ['word-wrap', 'typography/text-wrapping.css'],
  ['hyphens', 'typography/text-wrapping.css'],
  ['hyphenate-character', 'typography/text-wrapping.css'],
  ['hyphenate-limit-chars', 'typography/text-wrapping.css'],
  ['text-overflow', 'typography/text-wrapping.css'],
  ['line-clamp', 'typography/text-wrapping.css'],
  ['-webkit-line-clamp', 'typography/text-wrapping.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 20. Typography: Text Decoration (typography/text-decoration.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['text-decoration', 'typography/text-decoration.css'],
  ['text-decoration-line', 'typography/text-decoration.css'],
  ['text-decoration-color', 'typography/text-decoration.css'],
  ['text-decoration-style', 'typography/text-decoration.css'],
  ['text-decoration-thickness', 'typography/text-decoration.css'],
  ['text-decoration-skip-ink', 'typography/text-decoration.css'],
  ['text-underline-offset', 'typography/text-decoration.css'],
  ['text-underline-position', 'typography/text-decoration.css'],
  ['text-emphasis', 'typography/text-decoration.css'],
  ['text-emphasis-style', 'typography/text-decoration.css'],
  ['text-emphasis-color', 'typography/text-decoration.css'],
  ['text-emphasis-position', 'typography/text-decoration.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 21. Color: Color (color/color.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['color', 'color/color.css'],
  ['opacity', 'color/color.css'],
  ['color-scheme', 'color/color.css'],
  ['forced-color-adjust', 'color/color.css'],
  ['print-color-adjust', 'color/color.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 22. Color: Background (color/background.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['background', 'color/background.css'],
  ['background-color', 'color/background.css'],
  ['background-image', 'color/background.css'],
  ['background-position', 'color/background.css'],
  ['background-position-x', 'color/background.css'],
  ['background-position-y', 'color/background.css'],
  ['background-size', 'color/background.css'],
  ['background-repeat', 'color/background.css'],
  ['background-attachment', 'color/background.css'],
  ['background-clip', 'color/background.css'],
  ['background-origin', 'color/background.css'],
  ['background-blend-mode', 'color/background.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 23. Color: Gradient Text (color/gradient-text.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['-webkit-text-fill-color', 'color/gradient-text.css'],
  ['text-fill-color', 'color/gradient-text.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 24. Effects: Shadow (effects/shadow.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['box-shadow', 'effects/shadow.css'],
  ['text-shadow', 'effects/shadow.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 25. Effects: Filters (effects/filters.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['filter', 'effects/filters.css'],
  ['-webkit-filter', 'effects/filters.css'],
  ['backdrop-filter', 'effects/filters.css'],
  ['-webkit-backdrop-filter', 'effects/filters.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 26. Effects: Masking (effects/masking.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['mask', 'effects/masking.css'],
  ['-webkit-mask', 'effects/masking.css'],
  ['mask-image', 'effects/masking.css'],
  ['mask-mode', 'effects/masking.css'],
  ['mask-repeat', 'effects/masking.css'],
  ['mask-position', 'effects/masking.css'],
  ['mask-clip', 'effects/masking.css'],
  ['mask-origin', 'effects/masking.css'],
  ['mask-size', 'effects/masking.css'],
  ['mask-composite', 'effects/masking.css'],
  ['mask-type', 'effects/masking.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 27. Effects: Clipping (effects/clipping.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['clip', 'effects/clipping.css'],
  ['clip-path', 'effects/clipping.css'],
  ['-webkit-clip-path', 'effects/clipping.css'],
  ['clip-rule', 'effects/clipping.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 28. Transforms: Transform (transforms/transform.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['transform', 'transforms/transform.css'],
  ['-webkit-transform', 'transforms/transform.css'],
  ['transform-origin', 'transforms/transform.css'],
  ['transform-box', 'transforms/transform.css'],
  ['transform-style', 'transforms/transform.css'],
  ['translate', 'transforms/transform.css'],
  ['rotate', 'transforms/transform.css'],
  ['scale', 'transforms/transform.css'],
  ['perspective', 'transforms/transform.css'],
  ['perspective-origin', 'transforms/transform.css'],
  ['backface-visibility', 'transforms/transform.css'],
  ['will-change', 'transforms/transform.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 29. Transforms: Motion Path (transforms/motion-path.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['offset', 'transforms/motion-path.css'],
  ['offset-path', 'transforms/motion-path.css'],
  ['offset-distance', 'transforms/motion-path.css'],
  ['offset-rotate', 'transforms/motion-path.css'],
  ['offset-anchor', 'transforms/motion-path.css'],
  ['offset-position', 'transforms/motion-path.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 30. Motion: Transitions (motion/transitions.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['transition', 'motion/transitions.css'],
  ['-webkit-transition', 'motion/transitions.css'],
  ['transition-property', 'motion/transitions.css'],
  ['transition-duration', 'motion/transitions.css'],
  ['transition-timing-function', 'motion/transitions.css'],
  ['transition-delay', 'motion/transitions.css'],
  ['transition-behavior', 'motion/transitions.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 31. Motion: Animations (motion/animations.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['animation', 'motion/animations.css'],
  ['-webkit-animation', 'motion/animations.css'],
  ['animation-name', 'motion/animations.css'],
  ['animation-duration', 'motion/animations.css'],
  ['animation-timing-function', 'motion/animations.css'],
  ['animation-delay', 'motion/animations.css'],
  ['animation-iteration-count', 'motion/animations.css'],
  ['animation-direction', 'motion/animations.css'],
  ['animation-fill-mode', 'motion/animations.css'],
  ['animation-play-state', 'motion/animations.css'],
  ['animation-composition', 'motion/animations.css'],
  ['animation-timeline', 'motion/animations.css'],
  ['animation-range', 'motion/animations.css'],
  ['animation-range-start', 'motion/animations.css'],
  ['animation-range-end', 'motion/animations.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 32. Content: Generated (content/generated.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['content', 'content/generated.css'],
  ['quotes', 'content/generated.css'],
  ['counter-reset', 'content/generated.css'],
  ['counter-increment', 'content/generated.css'],
  ['counter-set', 'content/generated.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 33. Content: Lists (content/lists.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['list-style', 'content/lists.css'],
  ['list-style-type', 'content/lists.css'],
  ['list-style-position', 'content/lists.css'],
  ['list-style-image', 'content/lists.css'],
  ['marker-side', 'content/lists.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 34. Content: Replaced (content/replaced.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['object-fit', 'content/replaced.css'],
  ['object-position', 'content/replaced.css'],
  ['object-view-box', 'content/replaced.css'],
  ['image-rendering', 'content/replaced.css'],
  ['image-orientation', 'content/replaced.css'],
  ['image-resolution', 'content/replaced.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 35. Interaction: Pointer (interaction/pointer.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['cursor', 'interaction/pointer.css'],
  ['pointer-events', 'interaction/pointer.css'],
  ['touch-action', 'interaction/pointer.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 36. Interaction: Selection (interaction/selection.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['user-select', 'interaction/selection.css'],
  ['-webkit-user-select', 'interaction/selection.css'],
  ['user-modify', 'interaction/selection.css'],
  ['caret-color', 'interaction/selection.css'],
  ['accent-color', 'interaction/selection.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 37. Interaction: Form Controls (interaction/form-controls.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['appearance', 'interaction/form-controls.css'],
  ['-webkit-appearance', 'interaction/form-controls.css'],
  ['resize', 'interaction/form-controls.css'],
  ['field-sizing', 'interaction/form-controls.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 38. Internationalization: Direction (i18n/direction.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['direction', 'i18n/direction.css'],
  ['unicode-bidi', 'i18n/direction.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 39. Internationalization: Writing Mode (i18n/writing-mode.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['writing-mode', 'i18n/writing-mode.css'],
  ['text-orientation', 'i18n/writing-mode.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 40. Containment: Contain (containment/contain.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['contain', 'containment/contain.css'],
  ['contain-intrinsic-size', 'containment/contain.css'],
  ['contain-intrinsic-width', 'containment/contain.css'],
  ['contain-intrinsic-height', 'containment/contain.css'],
  ['contain-intrinsic-inline-size', 'containment/contain.css'],
  ['contain-intrinsic-block-size', 'containment/contain.css'],
  ['content-visibility', 'containment/contain.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 41. Containment: Container Queries (containment/container-queries.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['container', 'containment/container-queries.css'],
  ['container-name', 'containment/container-queries.css'],
  ['container-type', 'containment/container-queries.css']
]);

/**
 * Resolves target W3C module stylesheet for any CSS property name.
 */
export function resolveModuleForProperty(property) {
  const normalized = property.toLowerCase().trim();

  // Custom CSS properties (--*) semantic routing
  if (normalized.startsWith('--')) {
    if (normalized.includes('shape') || normalized.includes('radius')) return 'box-model/border.css';
    if (normalized.includes('height') || normalized.includes('width') || normalized.includes('size')) return 'box-model/sizing.css';
    if (normalized.includes('margin')) return 'box-model/margin.css';
    if (normalized.includes('padding') || normalized.includes('spacing') || normalized.includes('gutter')) return 'box-model/padding.css';
    if (normalized.includes('shadow') || normalized.includes('glow') || normalized.includes('elevation')) return 'effects/shadow.css';
    if (normalized.includes('display')) return 'layout/display.css';
    if (normalized.startsWith('--s') || normalized.startsWith('--t') || normalized.startsWith('--tr')) return 'effects/filters.css';
    return 'color/color.css';
  }

  // Special cases
  if (normalized === '-webkit-background-clip' || normalized === 'background-clip') {
    return 'color/background.css';
  }

  if (PROPERTY_TO_W3C_MODULE.has(normalized)) {
    return PROPERTY_TO_W3C_MODULE.get(normalized);
  }

  // Fallback heuristic by logical prefix
  if (normalized.startsWith('border')) return 'box-model/border.css';
  if (normalized.startsWith('outline')) return 'box-model/outline.css';
  if (normalized.startsWith('margin')) return 'box-model/margin.css';
  if (normalized.startsWith('padding')) return 'box-model/padding.css';
  if (normalized.startsWith('grid')) return 'layout/grid.css';
  if (normalized.startsWith('font')) return 'typography/font.css';
  if (normalized.startsWith('text-decoration')) return 'typography/text-decoration.css';
  if (normalized.startsWith('text-')) return 'typography/text-layout.css';
  if (normalized.startsWith('background')) return 'color/background.css';
  if (normalized.startsWith('transition')) return 'motion/transitions.css';
  if (normalized.startsWith('animation')) return 'motion/animations.css';
  if (normalized.startsWith('scroll-')) return 'overflow/scrolling.css';
  if (normalized.startsWith('scrollbar-')) return 'overflow/scrollbars.css';
  if (normalized.startsWith('container-')) return 'containment/container-queries.css';

  return null;
}
