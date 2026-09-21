# Socratic Localization & i18n Standards

The Socratic ecosystem operates in a multilingual environment supporting **Uzbek (uz)**, **Russian (ru)**, and **English (en)**.

---

## 1. Resource Files Architecture
- Resources are centralized in `src/Frontend/Core/Shared/Resources/`:
  - `ResourceRu.resx` — Russian (Default)
  - `ResourceUz.resx` — Uzbek (Latin)
  - `ResourceEn.resx` — English

---

## 2. Localization Rules in Blazor Components
- **No Hardcoded User Strings**: User-visible text must not be hardcoded in Razor markup.
- **Inject IStringLocalizer**:
  ```razor
  @inject IStringLocalizer L
  
  <h3>@L["Orders"]</h3>
  <md-filled-button>@L["Create"]</md-filled-button>
  ```
- **Fallback Strings**: When providing fallbacks, use `@(L?["Key"] ?? "Default")`.
- **Three-Way Synchronization**: When adding a new resource key, you MUST add it to all three files (`ResourceRu.resx`, `ResourceUz.resx`, `ResourceEn.resx`).
- **No Duplicate Keys**: Avoid duplicate key names inside any single `.resx` file (triggers `MSB3568` warnings).

---

## 3. Supported Cultures & Request Localization
- Supported cultures:
  - `ru-RU` (Russian - Default)
  - `uz-Latn-UZ` (Uzbek Latin)
  - `en-US` (English)
- ASP.NET Core middleware (`UseRequestLocalization`) resolves culture via:
  1. Cookie (`CookieRequestCultureProvider.DefaultCookieName = ".AspNetCore.Culture"`)
  2. Accept-Language header
- When switching language in Blazor components, navigate to or fetch `/api/culture/set?culture={code}&redirectUri={uri}` so both the server prerenderer (SSR) and client WASM stay in lockstep.
