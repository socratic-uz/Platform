# Socratic Theming & Accent Guidelines (Material Design 3)

This document establishes the architecture and implementation rules for themes (Dark, Light, System) and dynamic accent colors in the Socratic ecosystem.

---

## 1. Scope & Design Tokens

Socratic UI components are based on **Material Web 3** (`@material/web`). All components resolve styling through CSS Custom Properties starting with `--md-sys-color-*`.

### 1.1 Root Theme Selectors
- **Never** use bare `body` selectors in theme stylesheets.
- Themes MUST be scoped using attribute selectors:
  ```css
  /* Dark Theme */
  :root[data-theme="dark"], body.dark, [data-theme="dark"] {
    --md-sys-color-primary: rgb(112 220 188);
    --md-sys-color-surface: rgb(14 21 19);
    --md-sys-color-on-surface: rgb(222 228 225);
    /* ... */
  }

  /* Light Theme */
  :root[data-theme="light"], body.light, [data-theme="light"] {
    --md-sys-color-primary: rgb(1 107 93);
    --md-sys-color-surface: rgb(245 251 247);
    --md-sys-color-on-surface: rgb(23 29 27);
    /* ... */
  }
  ```

---

## 2. Preventing FOUC (Flash of Unstyled Content) in SSR

Because Socratic uses Blazor InteractiveAuto (prerendered on the server, then hydrated in the browser):
1. The user's theme selection MUST be persisted in a cookie named `Theme` (values: `Dark`, `Light`, `System`).
2. An inline synchronous script in the `<head>` of `App.razor` reads the cookie and immediately sets `document.documentElement.setAttribute('data-theme', theme.toLowerCase())` before any DOM elements are painted.
3. If `Theme == "System"`, inspect `window.matchMedia('(prefers-color-scheme: light)').matches` to resolve the current system mode.

---

## 3. Dynamic Accent Colors

When an organization or user customizes their brand accent color:
1. The accent color is represented as an integer RGB or 6-character HEX string (`#RRGGBB`).
2. Material Design 3 dynamic color generation computes the primary, secondary, container, and surface-tint variants based on HCT tonal palettes.
3. Injected styles must be written to a dedicated `<style id="socratic-theme-tokens">` tag targeting `:root`, overriding `--md-sys-color-primary`, `--md-sys-color-on-primary`, `--md-sys-color-primary-container`, etc.
4. Accent changes must trigger reactive updates across all active components via `ISettingsManager` and event notifications.
