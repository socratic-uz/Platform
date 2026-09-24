# Blazor UI Quality & Browser Verification Standards

## 1. Golden Principle: Actual Rendered UI Is Law
No UI task, bugfix, or feature in Socratic is considered complete merely because the Razor or CSS source code looks correct.
The actual rendered UI in the browser (DOM, computed styles, bounding boxes, scroll metrics) is the sole authority for layout correctness, responsive behavior, and accessibility.

---

## 2. Mandatory Verification Gates for UI Changes

Before marking any Blazor UI change as complete:

1. **Responsive Verification**:
   - Verify on mobile viewports (`320px`, `390px`, `430px`) and desktop (`1440px`).
   - `scrollWidth` must never exceed `clientWidth` (zero horizontal overflow).
2. **Dual-Theme Verification (Light & Dark)**:
   - Check the component in both **Light** and **Dark** themes.
   - Contrast between text/icons and backgrounds must satisfy WCAG 2.1 AA (>= 4.5:1 for standard text, >= 3:1 for large text and icons).
3. **Touch Targets (Mobile / POS / Kiosk)**:
   - Interactive elements (`button`, `a`, `md-outlined-button`, `md-icon-button`, inputs) must have a minimum bounding rect of **48x48px** (minimum **44x44px** under WCAG).
4. **Strict Design Token Enforcement**:
   - Do NOT use inline hardcoded HEX or RGB colors (e.g. `style="background: #1e293b;"` is prohibited).
   - Use Google Material Design 3 tokens: `var(--md-sys-color-surface-container)`, `var(--md-sys-color-primary)`, etc.
5. **No Visual Overlaps**:
   - Sibling elements in normal flow must not collide or overlap unintentionally.
6. **CSS Grid Only (Zero Flexbox)**:
   - `display: flex` is strictly forbidden. All layouts, stacks, rows, toolbars, and card structures must use **CSS Grid (`display: grid`)**.

---

## 3. Tooling & MCP Execution
Use the `socratic-ui-quality` MCP server:
- `inspect_responsive`: Deterministic check for horizontal overflow across all viewports.
- `inspect_component`: Deep inspection combining DOM metrics, touch targets, token usage, and Razor source context.
- `find_overlaps`: Mathematical bounding box overlap detector.
- `compare_states`: Dual-state and dual-theme visual and geometric regression testing.
- `analyze_accessibility`: axe-core WCAG audits.

---

## 4. Root Cause over Symptom Fixes
- If a button overflows a card, do **not** increase card width or hide overflow arbitrarily.
- Inspect `grid-template-columns`, `minmax()`, `grid-auto-flow`, and `padding`.
- Solve the architectural defect at the component level.

