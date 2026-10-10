/**
 * Socratic Platform - Comprehensive W3C CSS Module Taxonomy
 * Comprehensive map of ~300 standard W3C CSS properties & logical properties
 * mapped to Socratic's 41 W3C modular stylesheets.
 */

export const PROPERTY_TO_W3C_MODULE = new Map([
  // ──────────────────────────────────────────────────────────────────────────
  // 1. Box Model: Sizing (box-model/sizing.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['width', 'w3c/box-model/sizing.css'],
  ['min-width', 'w3c/box-model/sizing.css'],
  ['max-width', 'w3c/box-model/sizing.css'],
  ['height', 'w3c/box-model/sizing.css'],
  ['min-height', 'w3c/box-model/sizing.css'],
  ['max-height', 'w3c/box-model/sizing.css'],
  ['inline-size', 'w3c/box-model/sizing.css'],
  ['min-inline-size', 'w3c/box-model/sizing.css'],
  ['max-inline-size', 'w3c/box-model/sizing.css'],
  ['block-size', 'w3c/box-model/sizing.css'],
  ['min-block-size', 'w3c/box-model/sizing.css'],
  ['max-block-size', 'w3c/box-model/sizing.css'],
  ['box-sizing', 'w3c/box-model/sizing.css'],
  ['aspect-ratio', 'w3c/box-model/sizing.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 2. Box Model: Margin (box-model/margin.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['margin', 'w3c/box-model/margin.css'],
  ['margin-top', 'w3c/box-model/margin.css'],
  ['margin-right', 'w3c/box-model/margin.css'],
  ['margin-bottom', 'w3c/box-model/margin.css'],
  ['margin-left', 'w3c/box-model/margin.css'],
  ['margin-inline', 'w3c/box-model/margin.css'],
  ['margin-inline-start', 'w3c/box-model/margin.css'],
  ['margin-inline-end', 'w3c/box-model/margin.css'],
  ['margin-block', 'w3c/box-model/margin.css'],
  ['margin-block-start', 'w3c/box-model/margin.css'],
  ['margin-block-end', 'w3c/box-model/margin.css'],
  ['margin-trim', 'w3c/box-model/margin.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 3. Box Model: Padding (box-model/padding.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['padding', 'w3c/box-model/padding.css'],
  ['padding-top', 'w3c/box-model/padding.css'],
  ['padding-right', 'w3c/box-model/padding.css'],
  ['padding-bottom', 'w3c/box-model/padding.css'],
  ['padding-left', 'w3c/box-model/padding.css'],
  ['padding-inline', 'w3c/box-model/padding.css'],
  ['padding-inline-start', 'w3c/box-model/padding.css'],
  ['padding-inline-end', 'w3c/box-model/padding.css'],
  ['padding-block', 'w3c/box-model/padding.css'],
  ['padding-block-start', 'w3c/box-model/padding.css'],
  ['padding-block-end', 'w3c/box-model/padding.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 4. Box Model: Border (box-model/border.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['border', 'w3c/box-model/border.css'],
  ['border-width', 'w3c/box-model/border.css'],
  ['border-style', 'w3c/box-model/border.css'],
  ['border-color', 'w3c/box-model/border.css'],
  ['border-radius', 'w3c/box-model/border.css'],
  ['border-top-left-radius', 'w3c/box-model/border.css'],
  ['border-top-right-radius', 'w3c/box-model/border.css'],
  ['border-bottom-left-radius', 'w3c/box-model/border.css'],
  ['border-bottom-right-radius', 'w3c/box-model/border.css'],
  ['border-start-start-radius', 'w3c/box-model/border.css'],
  ['border-start-end-radius', 'w3c/box-model/border.css'],
  ['border-end-start-radius', 'w3c/box-model/border.css'],
  ['border-end-end-radius', 'w3c/box-model/border.css'],
  ['border-top', 'w3c/box-model/border.css'],
  ['border-top-width', 'w3c/box-model/border.css'],
  ['border-top-style', 'w3c/box-model/border.css'],
  ['border-top-color', 'w3c/box-model/border.css'],
  ['border-right', 'w3c/box-model/border.css'],
  ['border-right-width', 'w3c/box-model/border.css'],
  ['border-right-style', 'w3c/box-model/border.css'],
  ['border-right-color', 'w3c/box-model/border.css'],
  ['border-bottom', 'w3c/box-model/border.css'],
  ['border-bottom-width', 'w3c/box-model/border.css'],
  ['border-bottom-style', 'w3c/box-model/border.css'],
  ['border-bottom-color', 'w3c/box-model/border.css'],
  ['border-left', 'w3c/box-model/border.css'],
  ['border-left-width', 'w3c/box-model/border.css'],
  ['border-left-style', 'w3c/box-model/border.css'],
  ['border-left-color', 'w3c/box-model/border.css'],
  ['border-inline', 'w3c/box-model/border.css'],
  ['border-inline-start', 'w3c/box-model/border.css'],
  ['border-inline-end', 'w3c/box-model/border.css'],
  ['border-inline-width', 'w3c/box-model/border.css'],
  ['border-inline-start-width', 'w3c/box-model/border.css'],
  ['border-inline-end-width', 'w3c/box-model/border.css'],
  ['border-inline-style', 'w3c/box-model/border.css'],
  ['border-inline-start-style', 'w3c/box-model/border.css'],
  ['border-inline-end-style', 'w3c/box-model/border.css'],
  ['border-inline-color', 'w3c/box-model/border.css'],
  ['border-inline-start-color', 'w3c/box-model/border.css'],
  ['border-inline-end-color', 'w3c/box-model/border.css'],
  ['border-block', 'w3c/box-model/border.css'],
  ['border-block-start', 'w3c/box-model/border.css'],
  ['border-block-end', 'w3c/box-model/border.css'],
  ['border-block-width', 'w3c/box-model/border.css'],
  ['border-block-start-width', 'w3c/box-model/border.css'],
  ['border-block-end-width', 'w3c/box-model/border.css'],
  ['border-block-style', 'w3c/box-model/border.css'],
  ['border-block-start-style', 'w3c/box-model/border.css'],
  ['border-block-end-style', 'w3c/box-model/border.css'],
  ['border-block-color', 'w3c/box-model/border.css'],
  ['border-block-start-color', 'w3c/box-model/border.css'],
  ['border-block-end-color', 'w3c/box-model/border.css'],
  ['border-image', 'w3c/box-model/border.css'],
  ['border-image-source', 'w3c/box-model/border.css'],
  ['border-image-slice', 'w3c/box-model/border.css'],
  ['border-image-width', 'w3c/box-model/border.css'],
  ['border-image-outset', 'w3c/box-model/border.css'],
  ['border-image-repeat', 'w3c/box-model/border.css'],
  ['box-decoration-break', 'w3c/box-model/border.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 5. Box Model: Outline (box-model/outline.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['outline', 'w3c/box-model/outline.css'],
  ['outline-width', 'w3c/box-model/outline.css'],
  ['outline-style', 'w3c/box-model/outline.css'],
  ['outline-color', 'w3c/box-model/outline.css'],
  ['outline-offset', 'w3c/box-model/outline.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 6. Layout: Display (layout/display.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['display', 'w3c/layout/display.css'],
  ['visibility', 'w3c/layout/display.css'],
  ['float', 'w3c/layout/display.css'],
  ['clear', 'w3c/layout/display.css'],
  ['vertical-align', 'w3c/layout/display.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 7. Layout: Grid (layout/grid.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['grid', 'w3c/layout/grid.css'],
  ['grid-template', 'w3c/layout/grid.css'],
  ['grid-template-columns', 'w3c/layout/grid.css'],
  ['grid-template-rows', 'w3c/layout/grid.css'],
  ['grid-template-areas', 'w3c/layout/grid.css'],
  ['grid-auto-flow', 'w3c/layout/grid.css'],
  ['grid-auto-columns', 'w3c/layout/grid.css'],
  ['grid-auto-rows', 'w3c/layout/grid.css'],
  ['grid-column', 'w3c/layout/grid.css'],
  ['grid-column-start', 'w3c/layout/grid.css'],
  ['grid-column-end', 'w3c/layout/grid.css'],
  ['grid-row', 'w3c/layout/grid.css'],
  ['grid-row-start', 'w3c/layout/grid.css'],
  ['grid-row-end', 'w3c/layout/grid.css'],
  ['grid-area', 'w3c/layout/grid.css'],
  ['subgrid', 'w3c/layout/grid.css'],
  ['masonry-auto-flow', 'w3c/layout/grid.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 8. Layout: Alignment (layout/alignment.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['gap', 'w3c/layout/alignment.css'],
  ['row-gap', 'w3c/layout/alignment.css'],
  ['column-gap', 'w3c/layout/alignment.css'],
  ['justify-content', 'w3c/layout/alignment.css'],
  ['justify-items', 'w3c/layout/alignment.css'],
  ['justify-self', 'w3c/layout/alignment.css'],
  ['align-content', 'w3c/layout/alignment.css'],
  ['align-items', 'w3c/layout/alignment.css'],
  ['align-self', 'w3c/layout/alignment.css'],
  ['place-content', 'w3c/layout/alignment.css'],
  ['place-items', 'w3c/layout/alignment.css'],
  ['place-self', 'w3c/layout/alignment.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 9. Layout: Multi-column (layout/multi-column.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['columns', 'w3c/layout/multi-column.css'],
  ['column-count', 'w3c/layout/multi-column.css'],
  ['column-width', 'w3c/layout/multi-column.css'],
  ['column-rule', 'w3c/layout/multi-column.css'],
  ['column-rule-width', 'w3c/layout/multi-column.css'],
  ['column-rule-style', 'w3c/layout/multi-column.css'],
  ['column-rule-color', 'w3c/layout/multi-column.css'],
  ['column-fill', 'w3c/layout/multi-column.css'],
  ['column-span', 'w3c/layout/multi-column.css'],
  ['break-before', 'w3c/layout/multi-column.css'],
  ['break-after', 'w3c/layout/multi-column.css'],
  ['break-inside', 'w3c/layout/multi-column.css'],
  ['page-break-before', 'w3c/layout/multi-column.css'],
  ['page-break-after', 'w3c/layout/multi-column.css'],
  ['page-break-inside', 'w3c/layout/multi-column.css'],
  ['orphans', 'w3c/layout/multi-column.css'],
  ['widows', 'w3c/layout/multi-column.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 10. Layout: Table (layout/table.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['table-layout', 'w3c/layout/table.css'],
  ['border-collapse', 'w3c/layout/table.css'],
  ['border-spacing', 'w3c/layout/table.css'],
  ['caption-side', 'w3c/layout/table.css'],
  ['empty-cells', 'w3c/layout/table.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 11. Positioning: Position (positioning/position.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['position', 'w3c/positioning/position.css'],
  ['top', 'w3c/positioning/position.css'],
  ['right', 'w3c/positioning/position.css'],
  ['bottom', 'w3c/positioning/position.css'],
  ['left', 'w3c/positioning/position.css'],
  ['inset', 'w3c/positioning/position.css'],
  ['inset-inline', 'w3c/positioning/position.css'],
  ['inset-inline-start', 'w3c/positioning/position.css'],
  ['inset-inline-end', 'w3c/positioning/position.css'],
  ['inset-block', 'w3c/positioning/position.css'],
  ['inset-block-start', 'w3c/positioning/position.css'],
  ['inset-block-end', 'w3c/positioning/position.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 12. Positioning: Stacking (positioning/stacking.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['z-index', 'w3c/positioning/stacking.css'],
  ['isolation', 'w3c/positioning/stacking.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 13. Positioning: Anchor (positioning/anchor.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['anchor-name', 'w3c/positioning/anchor.css'],
  ['position-anchor', 'w3c/positioning/anchor.css'],
  ['position-area', 'w3c/positioning/anchor.css'],
  ['position-try', 'w3c/positioning/anchor.css'],
  ['position-try-options', 'w3c/positioning/anchor.css'],
  ['position-try-order', 'w3c/positioning/anchor.css'],
  ['position-visibility', 'w3c/positioning/anchor.css'],
  ['anchor-scope', 'w3c/positioning/anchor.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 14. Overflow: Overflow (overflow/overflow.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['overflow', 'w3c/overflow/overflow.css'],
  ['overflow-x', 'w3c/overflow/overflow.css'],
  ['overflow-y', 'w3c/overflow/overflow.css'],
  ['overflow-block', 'w3c/overflow/overflow.css'],
  ['overflow-inline', 'w3c/overflow/overflow.css'],
  ['overflow-clip-margin', 'w3c/overflow/overflow.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 15. Overflow: Scrolling (overflow/scrolling.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['scroll-behavior', 'w3c/overflow/scrolling.css'],
  ['overscroll-behavior', 'w3c/overflow/scrolling.css'],
  ['overscroll-behavior-x', 'w3c/overflow/scrolling.css'],
  ['overscroll-behavior-y', 'w3c/overflow/scrolling.css'],
  ['overscroll-behavior-block', 'w3c/overflow/scrolling.css'],
  ['overscroll-behavior-inline', 'w3c/overflow/scrolling.css'],
  ['scroll-margin', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-top', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-right', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-bottom', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-left', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-inline', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-inline-start', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-inline-end', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-block', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-block-start', 'w3c/overflow/scrolling.css'],
  ['scroll-margin-block-end', 'w3c/overflow/scrolling.css'],
  ['scroll-padding', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-top', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-right', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-bottom', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-left', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-inline', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-inline-start', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-inline-end', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-block', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-block-start', 'w3c/overflow/scrolling.css'],
  ['scroll-padding-block-end', 'w3c/overflow/scrolling.css'],
  ['scroll-snap-type', 'w3c/overflow/scrolling.css'],
  ['scroll-snap-align', 'w3c/overflow/scrolling.css'],
  ['scroll-snap-stop', 'w3c/overflow/scrolling.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 16. Overflow: Scrollbars (overflow/scrollbars.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['scrollbar-width', 'w3c/overflow/scrollbars.css'],
  ['scrollbar-color', 'w3c/overflow/scrollbars.css'],
  ['scrollbar-gutter', 'w3c/overflow/scrollbars.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 17. Typography: Font (typography/font.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['font', 'w3c/typography/font.css'],
  ['font-family', 'w3c/typography/font.css'],
  ['font-size', 'w3c/typography/font.css'],
  ['font-size-adjust', 'w3c/typography/font.css'],
  ['font-weight', 'w3c/typography/font.css'],
  ['font-style', 'w3c/typography/font.css'],
  ['font-display', 'w3c/typography/font.css'],
  ['font-stretch', 'w3c/typography/font.css'],
  ['font-variant', 'w3c/typography/font.css'],
  ['font-variant-caps', 'w3c/typography/font.css'],
  ['font-variant-numeric', 'w3c/typography/font.css'],
  ['font-variant-alternates', 'w3c/typography/font.css'],
  ['font-variant-east-asian', 'w3c/typography/font.css'],
  ['font-variant-ligatures', 'w3c/typography/font.css'],
  ['font-variant-position', 'w3c/typography/font.css'],
  ['font-feature-settings', 'w3c/typography/font.css'],
  ['font-variation-settings', 'w3c/typography/font.css'],
  ['font-kerning', 'w3c/typography/font.css'],
  ['font-optical-sizing', 'w3c/typography/font.css'],
  ['font-palette', 'w3c/typography/font.css'],
  ['font-synthesis', 'w3c/typography/font.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 18. Typography: Text Layout (typography/text-layout.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['line-height', 'w3c/typography/text-layout.css'],
  ['-webkit-font-smoothing', 'w3c/typography/text-layout.css'],
  ['-moz-osx-font-smoothing', 'w3c/typography/text-layout.css'],
  ['text-rendering', 'w3c/typography/text-layout.css'],
  ['-webkit-text-size-adjust', 'w3c/typography/text-layout.css'],
  ['text-align', 'w3c/typography/text-layout.css'],
  ['text-align-last', 'w3c/typography/text-layout.css'],
  ['text-justify', 'w3c/typography/text-layout.css'],
  ['text-indent', 'w3c/typography/text-layout.css'],
  ['letter-spacing', 'w3c/typography/text-layout.css'],
  ['word-spacing', 'w3c/typography/text-layout.css'],
  ['text-transform', 'w3c/typography/text-layout.css'],
  ['tab-size', 'w3c/typography/text-layout.css'],
  ['ruby-align', 'w3c/typography/text-layout.css'],
  ['ruby-position', 'w3c/typography/text-layout.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 19. Typography: Text Wrapping (typography/text-wrapping.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['white-space', 'w3c/typography/text-wrapping.css'],
  ['white-space-collapse', 'w3c/typography/text-wrapping.css'],
  ['text-wrap', 'w3c/typography/text-wrapping.css'],
  ['text-wrap-mode', 'w3c/typography/text-wrapping.css'],
  ['text-wrap-style', 'w3c/typography/text-wrapping.css'],
  ['word-break', 'w3c/typography/text-wrapping.css'],
  ['overflow-wrap', 'w3c/typography/text-wrapping.css'],
  ['word-wrap', 'w3c/typography/text-wrapping.css'],
  ['hyphens', 'w3c/typography/text-wrapping.css'],
  ['hyphenate-character', 'w3c/typography/text-wrapping.css'],
  ['hyphenate-limit-chars', 'w3c/typography/text-wrapping.css'],
  ['text-overflow', 'w3c/typography/text-wrapping.css'],
  ['line-clamp', 'w3c/typography/text-wrapping.css'],
  ['-webkit-line-clamp', 'w3c/typography/text-wrapping.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 20. Typography: Text Decoration (typography/text-decoration.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['text-decoration', 'w3c/typography/text-decoration.css'],
  ['text-decoration-line', 'w3c/typography/text-decoration.css'],
  ['text-decoration-color', 'w3c/typography/text-decoration.css'],
  ['text-decoration-style', 'w3c/typography/text-decoration.css'],
  ['text-decoration-thickness', 'w3c/typography/text-decoration.css'],
  ['text-decoration-skip-ink', 'w3c/typography/text-decoration.css'],
  ['text-underline-offset', 'w3c/typography/text-decoration.css'],
  ['text-underline-position', 'w3c/typography/text-decoration.css'],
  ['text-emphasis', 'w3c/typography/text-decoration.css'],
  ['text-emphasis-style', 'w3c/typography/text-decoration.css'],
  ['text-emphasis-color', 'w3c/typography/text-decoration.css'],
  ['text-emphasis-position', 'w3c/typography/text-decoration.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 21. Color: Color (color/color.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['color', 'w3c/color/color.css'],
  ['color-scheme', 'w3c/color/color.css'],
  ['forced-color-adjust', 'w3c/color/color.css'],
  ['-webkit-text-fill-color', 'w3c/color/color.css'],
  ['text-fill-color', 'w3c/color/color.css'],
  ['print-color-adjust', 'w3c/color/color.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 22. Color: Background (color/background.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['background', 'w3c/color/background.css'],
  ['background-color', 'w3c/color/background.css'],
  ['background-image', 'w3c/color/background.css'],
  ['background-position', 'w3c/color/background.css'],
  ['background-position-x', 'w3c/color/background.css'],
  ['background-position-y', 'w3c/color/background.css'],
  ['background-size', 'w3c/color/background.css'],
  ['background-repeat', 'w3c/color/background.css'],
  ['background-attachment', 'w3c/color/background.css'],
  ['background-clip', 'w3c/color/background.css'],
  ['background-origin', 'w3c/color/background.css'],
  ['background-blend-mode', 'w3c/color/background.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // ──────────────────────────────────────────────────────────────────────────

  // ──────────────────────────────────────────────────────────────────────────
  // 24. Effects: Shadow (effects/shadow.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['box-shadow', 'w3c/effects/shadow.css'],
  ['opacity', 'w3c/effects/shadow.css'],
  ['text-shadow', 'w3c/effects/shadow.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 25. Effects: Filters (effects/filters.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['filter', 'w3c/effects/filters.css'],
  ['-webkit-filter', 'w3c/effects/filters.css'],
  ['backdrop-filter', 'w3c/effects/filters.css'],
  ['-webkit-backdrop-filter', 'w3c/effects/filters.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 26. Effects: Masking (effects/masking.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['mask', 'w3c/effects/masking.css'],
  ['-webkit-mask', 'w3c/effects/masking.css'],
  ['mask-image', 'w3c/effects/masking.css'],
  ['mask-mode', 'w3c/effects/masking.css'],
  ['mask-repeat', 'w3c/effects/masking.css'],
  ['mask-position', 'w3c/effects/masking.css'],
  ['mask-clip', 'w3c/effects/masking.css'],
  ['mask-origin', 'w3c/effects/masking.css'],
  ['mask-size', 'w3c/effects/masking.css'],
  ['mask-composite', 'w3c/effects/masking.css'],
  ['mask-type', 'w3c/effects/masking.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 27. Effects: Clipping (effects/clipping.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['clip', 'w3c/effects/clipping.css'],
  ['clip-path', 'w3c/effects/clipping.css'],
  ['-webkit-clip-path', 'w3c/effects/clipping.css'],
  ['clip-rule', 'w3c/effects/clipping.css'],
  ['shape-outside', 'w3c/effects/clipping.css'],
  ['shape-margin', 'w3c/effects/clipping.css'],
  ['shape-image-threshold', 'w3c/effects/clipping.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 28. Transforms: Transform (transforms/transform.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['transform', 'w3c/transforms/transform.css'],
  ['-webkit-transform', 'w3c/transforms/transform.css'],
  ['transform-origin', 'w3c/transforms/transform.css'],
  ['transform-box', 'w3c/transforms/transform.css'],
  ['transform-style', 'w3c/transforms/transform.css'],
  ['translate', 'w3c/transforms/transform.css'],
  ['rotate', 'w3c/transforms/transform.css'],
  ['scale', 'w3c/transforms/transform.css'],
  ['perspective', 'w3c/transforms/transform.css'],
  ['perspective-origin', 'w3c/transforms/transform.css'],
  ['backface-visibility', 'w3c/transforms/transform.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 29. Transforms: Motion Path (transforms/motion-path.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['offset', 'w3c/transforms/motion-path.css'],
  ['offset-path', 'w3c/transforms/motion-path.css'],
  ['offset-distance', 'w3c/transforms/motion-path.css'],
  ['offset-rotate', 'w3c/transforms/motion-path.css'],
  ['offset-anchor', 'w3c/transforms/motion-path.css'],
  ['offset-position', 'w3c/transforms/motion-path.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 30. Motion: Transitions (motion/transitions.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['transition', 'w3c/motion/transitions.css'],
  ['-webkit-transition', 'w3c/motion/transitions.css'],
  ['transition-property', 'w3c/motion/transitions.css'],
  ['transition-duration', 'w3c/motion/transitions.css'],
  ['transition-timing-function', 'w3c/motion/transitions.css'],
  ['transition-delay', 'w3c/motion/transitions.css'],
  ['transition-behavior', 'w3c/motion/transitions.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 31. Motion: Animations (motion/animations.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['animation', 'w3c/motion/animations.css'],
  ['-webkit-animation', 'w3c/motion/animations.css'],
  ['animation-name', 'w3c/motion/animations.css'],
  ['animation-duration', 'w3c/motion/animations.css'],
  ['animation-timing-function', 'w3c/motion/animations.css'],
  ['animation-delay', 'w3c/motion/animations.css'],
  ['animation-iteration-count', 'w3c/motion/animations.css'],
  ['animation-direction', 'w3c/motion/animations.css'],
  ['animation-fill-mode', 'w3c/motion/animations.css'],
  ['animation-play-state', 'w3c/motion/animations.css'],
  ['animation-composition', 'w3c/motion/animations.css'],
  ['animation-timeline', 'w3c/motion/animations.css'],
  ['animation-range', 'w3c/motion/animations.css'],
  ['animation-range-start', 'w3c/motion/animations.css'],
  ['animation-range-end', 'w3c/motion/animations.css'],
  ['scroll-timeline', 'w3c/motion/animations.css'],
  ['scroll-timeline-name', 'w3c/motion/animations.css'],
  ['scroll-timeline-axis', 'w3c/motion/animations.css'],
  ['view-timeline', 'w3c/motion/animations.css'],
  ['view-timeline-name', 'w3c/motion/animations.css'],
  ['view-timeline-axis', 'w3c/motion/animations.css'],
  ['view-timeline-inset', 'w3c/motion/animations.css'],
  ['timeline-scope', 'w3c/motion/animations.css'],
  ['view-transition-name', 'w3c/motion/animations.css'],
  ['view-transition-class', 'w3c/motion/animations.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 32. Content: Generated (content/generated.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['content', 'w3c/content/generated.css'],
  ['quotes', 'w3c/content/generated.css'],
  ['counter-reset', 'w3c/content/generated.css'],
  ['counter-increment', 'w3c/content/generated.css'],
  ['counter-set', 'w3c/content/generated.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 33. Content: Lists (content/lists.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['list-style', 'w3c/content/lists.css'],
  ['list-style-type', 'w3c/content/lists.css'],
  ['list-style-position', 'w3c/content/lists.css'],
  ['list-style-image', 'w3c/content/lists.css'],
  ['marker-side', 'w3c/content/lists.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 34. Content: Replaced (content/replaced.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['object-fit', 'w3c/content/replaced.css'],
  ['object-position', 'w3c/content/replaced.css'],
  ['object-view-box', 'w3c/content/replaced.css'],
  ['image-rendering', 'w3c/content/replaced.css'],
  ['image-orientation', 'w3c/content/replaced.css'],
  ['image-resolution', 'w3c/content/replaced.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 35. Interaction: Pointer (interaction/pointer.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['cursor', 'w3c/interaction/pointer.css'],
  ['pointer-events', 'w3c/interaction/pointer.css'],
  ['touch-action', 'w3c/interaction/pointer.css'],
  ['-webkit-tap-highlight-color', 'w3c/interaction/pointer.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 36. Interaction: Selection (interaction/selection.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['user-select', 'w3c/interaction/selection.css'],
  ['-webkit-user-select', 'w3c/interaction/selection.css'],
  ['user-modify', 'w3c/interaction/selection.css'],
  ['caret-color', 'w3c/interaction/selection.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 37. Interaction: Form Controls (interaction/form-controls.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['appearance', 'w3c/interaction/form-controls.css'],
  ['-webkit-appearance', 'w3c/interaction/form-controls.css'],
  ['resize', 'w3c/interaction/form-controls.css'],
  ['field-sizing', 'w3c/interaction/form-controls.css'],
  ['accent-color', 'w3c/interaction/form-controls.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 38. Internationalization: Direction (i18n/direction.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['direction', 'w3c/i18n/direction.css'],
  ['unicode-bidi', 'w3c/i18n/direction.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 39. Internationalization: Writing Mode (i18n/writing-mode.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['writing-mode', 'w3c/i18n/writing-mode.css'],
  ['text-orientation', 'w3c/i18n/writing-mode.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 40. Containment: Contain (containment/contain.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['contain', 'w3c/containment/contain.css'],
  ['contain-intrinsic-size', 'w3c/containment/contain.css'],
  ['contain-intrinsic-width', 'w3c/containment/contain.css'],
  ['contain-intrinsic-height', 'w3c/containment/contain.css'],
  ['contain-intrinsic-inline-size', 'w3c/containment/contain.css'],
  ['contain-intrinsic-block-size', 'w3c/containment/contain.css'],
  ['content-visibility', 'w3c/containment/contain.css'],
  ['will-change', 'w3c/containment/contain.css'],

  // ──────────────────────────────────────────────────────────────────────────
  // 41. Containment: Container Queries (containment/container-queries.css)
  // ──────────────────────────────────────────────────────────────────────────
  ['container', 'w3c/containment/container-queries.css'],
  ['container-name', 'w3c/containment/container-queries.css'],
  ['container-type', 'w3c/containment/container-queries.css']
]);

/**
 * Resolves target W3C module stylesheet for any CSS property name.
 */
export function resolveModuleForProperty(property) {
  const normalized = property.toLowerCase().trim();

  // Custom CSS properties (--*) semantic routing
  if (normalized.startsWith('--')) {
    if (normalized.includes('shape') || normalized.includes('radius')) return 'w3c/box-model/border.css';
    if (normalized.includes('height') || normalized.includes('width') || normalized.includes('size')) return 'w3c/box-model/sizing.css';
    if (normalized.includes('margin')) return 'w3c/box-model/margin.css';
    if (normalized.includes('padding') || normalized.includes('spacing') || normalized.includes('gutter')) return 'w3c/box-model/padding.css';
    if (normalized.includes('shadow') || normalized.includes('glow') || normalized.includes('elevation')) return 'w3c/effects/shadow.css';
    if (normalized.includes('motion') || normalized.includes('easing') || normalized.includes('duration')) return 'w3c/motion/transitions.css';
    if (normalized.includes('display')) return 'w3c/layout/display.css';
    if (normalized.startsWith('--s') || normalized.startsWith('--t') || normalized.startsWith('--tr')) return 'w3c/effects/filters.css';
    return 'w3c/color/color.css';
  }

  // Special cases
  if (normalized === '-webkit-background-clip' || normalized === 'background-clip') {
    return 'w3c/color/background.css';
  }

  if (PROPERTY_TO_W3C_MODULE.has(normalized)) {
    return PROPERTY_TO_W3C_MODULE.get(normalized);
  }

  // Fallback heuristic by logical prefix
  if (normalized.startsWith('border')) return 'w3c/box-model/border.css';
  if (normalized.startsWith('outline')) return 'w3c/box-model/outline.css';
  if (normalized.startsWith('margin')) return 'w3c/box-model/margin.css';
  if (normalized.startsWith('padding')) return 'w3c/box-model/padding.css';
  if (normalized.startsWith('grid')) return 'w3c/layout/grid.css';
  if (normalized.startsWith('font')) return 'w3c/typography/font.css';
  if (normalized.startsWith('text-decoration')) return 'w3c/typography/text-decoration.css';
  if (normalized.startsWith('text-')) return 'w3c/typography/text-layout.css';
  if (normalized.startsWith('background')) return 'w3c/color/background.css';
  if (normalized.startsWith('transition')) return 'w3c/motion/transitions.css';
  if (normalized.startsWith('animation')) return 'w3c/motion/animations.css';
  if (normalized.startsWith('scroll-')) return 'w3c/overflow/scrolling.css';
  if (normalized.startsWith('scrollbar-')) return 'w3c/overflow/scrollbars.css';
  if (normalized.startsWith('container-')) return 'w3c/containment/container-queries.css';

  return null;
}
