---
name: socratic-audits
description: Comprehensive quality and compliance scanner for Socratic codebase health. USE FOR: auditing and detecting unmigrated native HTML elements (buttons, inputs) needing Material Web 3 components; detecting hardcoded colors, HEX values, and missing design tokens; detecting hardcoded secrets, IP addresses, credentials, and API keys; detecting missing UI translations and .resx key asymmetry across RU, UZ, EN cultures. DO NOT USE FOR: fixing compilation build errors.
---

# Socratic Quality, Design & Security Audits

This skill provides automated PowerShell scanners to maintain architectural integrity, security, design consistency, and localization synchronization across Socratic.

---

## 1. 🎨 Material Web Component Audit

Scans `.razor` files to detect native HTML elements (`<button>`, `<input>`, `<select>`, `<dialog>`) or third-party wrappers (MudBlazor, Fluent) that must be migrated to `@material/web` custom elements.

```powershell
pwsh .agents/skills/socratic-audits/scripts/Audit-MaterialComponents.ps1 -ExportJson
```

### Replacement Reference:
- `<button class="btn">` $\rightarrow$ `<md-filled-button>` or `<md-outlined-button>`
- `<input type="text">` $\rightarrow$ `<md-outlined-text-field>`
- `<input type="checkbox">` $\rightarrow$ `<md-checkbox>`
- `<select>` $\rightarrow$ `<md-outlined-select>`
- `<dialog>` $\rightarrow$ `<md-dialog>`

---

## 2. 🌈 Theme Tokens & Hardcoded Colors Audit

Scans `.razor` and `.razor.css` files for hardcoded HEX colors (`#ffffff`, `#1a1a1a`), `rgb(...)`, `hsl(...)`, and named CSS colors (`black`, `red`) that violate the Material Design 3 token system.

```powershell
pwsh .agents/skills/socratic-audits/scripts/Audit-ThemeColors.ps1 -ExportJson
```

### Replacement Reference:
- Backgrounds $\rightarrow$ `var(--md-sys-color-surface)` / `var(--md-sys-color-surface-container)`
- Text color $\rightarrow$ `var(--md-sys-color-on-surface)` / `var(--md-sys-color-on-surface-variant)`
- Primary brand $\rightarrow$ `var(--md-sys-color-primary)` / `var(--md-sys-color-on-primary)`
- Borders $\rightarrow$ `var(--md-sys-color-outline)` / `var(--md-sys-color-outline-variant)`

---

## 3. 🔒 Hardcode & Security Audit

Scans all C# and Razor source code for sensitive leaks:
- Hardcoded API keys, JWT secrets, database passwords.
- Absolute file paths (`C:\Users\...`, `/var/log/...`).
- Hardcoded `localhost` or IPv4 addresses that break containerized deployments.

```powershell
pwsh .agents/skills/socratic-audits/scripts/Audit-Hardcode.ps1 -ExportJson
```

---

## 4. 🌐 Localization (i18n) Synchronization Audit

Checks that all localization keys are symmetrically defined across all 3 supported languages (`ResourceRu.resx`, `ResourceUz.resx`, `ResourceEn.resx`) and detects raw, unlocalized strings in Blazor markup:

```powershell
pwsh .agents/skills/socratic-audits/scripts/Audit-Localization.ps1 -CheckRazor -ExportJson
```
