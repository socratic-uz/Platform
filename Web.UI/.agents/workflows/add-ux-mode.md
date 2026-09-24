---
description: Step-by-step guide for implementing or extending one of the 28 Universal Product UX Modes in Socratic.
---

# Workflow: Implementing & Extending a Universal Product UX Mode

Follow this step-by-step workflow when adding, customizing, or verifying a `ProductUxMode` experience in Socratic.

---

## 1. Verify Mode Definition
1. Inspect the enum definition with CodeGraph:
   - Run `codegraph_node(name="ProductUxMode")` to view all 28 enum constants, values (1–28), emoji icons, and titles.
   - Alternatively, open [SharedKernel/ValueObjects/ProductUxMode.cs](file:///c:/Users/owner/source/repos/Socratic/src/Shared/SharedKernel/ValueObjects/ProductUxMode.cs).
2. Confirm the expected metadata schema required for this commercial scenario.

---

## 2. Define Metadata Model & AOT Serialization
1. Create or update the mode DTO in `src/Frontend/Retail/Commerce/Modules/<Domain>/Models/<Mode>Metadata.cs`.
2. Register the DTO in `src/Frontend/Retail/Commerce/Serialization/AotJsonContext.cs` with `[JsonSerializable(typeof(<Mode>Metadata))]` to prevent Native AOT trimming crashes.

---

## 3. Implement Experience Dialog Module
1. Create or modify the Blazor module component in `src/Frontend/Retail/Commerce/Modules/<Domain>/<Mode>Module.razor`.
2. Implement standard parameters:
   ```razor
   [Parameter] public Product? Product { get; set; }
   [Parameter] public EventCallback<ExperienceResult> OnExperienceConfirmed { get; set; }
   [Parameter] public EventCallback OnExperienceCancelled { get; set; }
   ```
3. Use Material Web 3 components (`<Button>`, `<TextField>`, `<Checkbox>`, `<Select>`) and Material design tokens (`var(--md-sys-color-*)`).
4. Wrap user-visible labels in `@L["Key"]`.

---

## 4. Register in ProductModuleRegistry
1. Use `codegraph_node(name="ProductModuleRegistry")` to locate the active registry implementation across submodules.
2. Map `ProductUxMode.<ModeName>.Value` to `typeof(<Mode>Module)` inside `ModuleMap`.
3. Run `codegraph_impact(symbol="ProductUxMode")` to verify that no consumers of UX modes are broken.

---

## 5. Add 3-Way Localization
1. Add translation keys to:
   - `src/Frontend/Platform/Shared/DesignSystem/Layout/Resources/ResourceRu.resx`
   - `src/Frontend/Platform/Shared/DesignSystem/Layout/Resources/ResourceUz.resx`
   - `src/Frontend/Platform/Shared/DesignSystem/Layout/Resources/ResourceEn.resx`

---

## 6. Seed Demo Data & Verification
1. Add a sample product in `SocraticSeederService.cs` with `CustomAttributes["UxMode"] = "<value>"`.
2. Run validation scripts:
   ```powershell
   pwsh -File scripts/Audit-UxModes.ps1
   pwsh -File scripts/Audit-Localization.ps1
   dotnet build src/Frontend/Platform/Web.UI/Web.UI/Web.UI.csproj
   ```
