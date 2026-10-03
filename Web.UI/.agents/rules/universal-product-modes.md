# Socratic 28 Commerce Features (Commerce Frontend Module) Architecture Rules

This document establishes architectural standards for the 28 commercial features (`ProductUxMode`) within the **Commerce Frontend Module**, representing all trade and services in Socratic.

---

## 0. Единая терминология (Ubiquitous Language)

1. **Фронтенд-модуль (`Frontend Module`)**: крупная независимая доменная подсистема / репозиторий (`Commerce`, `POS`, `Kiosk`, `Vision`).
2. **Коммерческая фича (`Commerce Feature`)**: 28 интерактивных сценариев и диалогов внутри модуля `Commerce` (`HoReCa`, `Booking`, `Seats`, `Hotel`, `Auction`, `Rental` и т.д.).
3. **Компоненты и виджеты (`Components & Widgets`)**: атомарные элементы интерфейса из библиотеки `Material.Web` (`MaterialDialog`, `ResourceBookingPicker`, `MaterialButton`).

---

## 1. Single Entity Principle (NoSQL & ScyllaDB)

- **Strict Rule**: In ScyllaDB, **NEVER** create separate tables for distinct commercial domains (e.g., do NOT create `Hotels`, `Vehicles`, `Tickets`, `Auctions`, or `Services` tables).
- **Universal Entity**: All 28 commercial variations are stored as the single `Product` entity in the `Shopping` service (`products` table).
- **Domain Specialization**: Specialization is achieved purely through:
  1. `ProductUxModeValue` (or `CustomAttributes["UxMode"]`) storing the integer feature id (1–28).
  2. `Product.Metadata` storing domain-specific configuration as a JSON string.

---

## 2. Native AOT & Zero-Reflection Feature Registry

- The Blazor experience dialog mapping is located in [ProductModuleRegistry.cs](file:///c:/Users/owner/source/repos/Socratic/src/Frontend/Retail/Commerce/Architecture/ProductModuleRegistry.cs).
- **Never use dynamic reflection** (`Type.GetType`, `Activator.CreateInstance`) to load UI features.
- All feature mappings must be statically registered in the compile-time `FrozenDictionary<int, Type> ModuleMap`.
- Feature 1 (`CatalogItem`) is the standard non-modal catalog product and resolves to `null`. Features 2 through 28 map to their respective Blazor dialog features in `UI.Shared.Modules.*`.

---

## 3. Commerce Feature Contract

Every dialog feature rendered by `UniversalProductExperienceRenderer` must follow the experience contract:
```razor
@code {
    [Parameter] public Product? Product { get; set; }
    [Parameter] public EventCallback<ExperienceResult> OnExperienceConfirmed { get; set; }
    [Parameter] public EventCallback OnExperienceCancelled { get; set; }
}
```
- Features must read their domain configuration from `Product.Metadata` using source-generated `AotJsonContext`.
- Upon confirmation, features emit an `ExperienceResult` containing configured modifiers, price adjustments, and booking slots, which are forwarded to `UniversalCheckoutPanel`.

---

## 4. Multilingual & SEO Requirements for Commerce Features

1. **Translations**: Every commerce feature must have localized title and description keys in `ResourceRu.resx`, `ResourceUz.resx`, and `ResourceEn.resx`.
2. **Schema.org Structured Data**:
   - `HotelAccommodation` (5) ➔ `@type: "HotelRoom"` / `"Hotel"`
   - `TicketBooking` (4) ➔ `@type: "Event"` / `"EventReservation"`
   - `VehicleRental` (6) ➔ `@type: "RentalCarReservation"`
   - `BookableResource` (3) / `HourlyService` (28) ➔ `@type: "Service"`
   - `CatalogItem` (1) / `DigitalGoods` (10) ➔ `@type: "Product"`
