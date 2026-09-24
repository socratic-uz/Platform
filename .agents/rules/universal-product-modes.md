# Socratic 28 Universal Product UX Modes Architecture Rules

This document establishes architectural standards for the 28 commercial modes (`ProductUxMode`) representing all trade and services in Socratic.

---

## 1. Single Entity Principle (NoSQL & ScyllaDB)

- **Strict Rule**: In ScyllaDB, **NEVER** create separate tables for distinct commercial domains (e.g., do NOT create `Hotels`, `Vehicles`, `Tickets`, `Auctions`, or `Services` tables).
- **Universal Entity**: All 28 commercial variations are stored as the single `Product` entity in the `Shopping` service (`products` table).
- **Domain Specialization**: Specialization is achieved purely through:
  1. `ProductUxModeValue` (or `CustomAttributes["UxMode"]`) storing the integer mode (1–28).
  2. `Product.Metadata` storing domain-specific configuration as a JSON string.

---

## 2. Native AOT & Zero-Reflection Module Registry

- The Blazor experience dialog mapping is located in [ProductModuleRegistry.cs](file:///c:/Users/owner/source/repos/Socratic/src/Frontend/Retail/Commerce/Architecture/ProductModuleRegistry.cs).
- **Never use dynamic reflection** (`Type.GetType`, `Activator.CreateInstance`) to load UI modules.
- All module mappings must be statically registered in the compile-time `FrozenDictionary<int, Type> ModuleMap`.
- Mode 1 (`CatalogItem`) is the standard non-modal catalog product and resolves to `null`. Modes 2 through 28 map to their respective Blazor modules in `UI.Shared.Modules.*`.

---

## 3. Experience Module Contract

Every dialog module rendered by `UniversalProductExperienceRenderer` must follow the experience contract:
```razor
@code {
    [Parameter] public Product? Product { get; set; }
    [Parameter] public EventCallback<ExperienceResult> OnExperienceConfirmed { get; set; }
    [Parameter] public EventCallback OnExperienceCancelled { get; set; }
}
```
- Modules must read their domain configuration from `Product.Metadata` using source-generated `AotJsonContext`.
- Upon confirmation, modules emit an `ExperienceResult` containing configured modifiers, price adjustments, and booking slots, which are forwarded to `UniversalCheckoutPanel`.

---

## 4. Multilingual & SEO Requirements for UX Modes

1. **Translations**: Every UX mode must have localized title and description keys in `ResourceRu.resx`, `ResourceUz.resx`, and `ResourceEn.resx`.
2. **Schema.org Structured Data**:
   - `HotelAccommodation` (5) ➔ `@type: "HotelRoom"` / `"Hotel"`
   - `TicketBooking` (4) ➔ `@type: "Event"` / `"EventReservation"`
   - `VehicleRental` (6) ➔ `@type: "RentalCarReservation"`
   - `BookableResource` (3) / `HourlyService` (28) ➔ `@type: "Service"`
   - `CatalogItem` (1) / `DigitalGoods` (10) ➔ `@type: "Product"`
