---
name: socratic-retail-fintech
description: Business logic guide for Socratic retail, commerce, and FinTech systems. USE FOR: implementing or extending the 28 Universal Product UX Modes in UniversalProductExperienceRenderer; integrating payment gateways (Uzcard, Humo, Visa/Mastercard), installments (Nasiya), and fiscal receipts (OFD / Soliq ГНК РУз); managing customer service live chat with real-time gRPC streaming; and integrating Leaflet/OpenStreetMap geospatial branches and places. DO NOT USE FOR: low-level ESC/POS byte commands or raw CSS styling.
---

# Socratic Retail & FinTech: 28 UX Modes, Payments, Fiscalization & Maps

The commercial engine of Socratic unites catalog adaptability, multi-rail payments, Uzbek fiscal regulation, customer messaging, and geospatial places.

---

## 1. 🛍️ 28 Universal Product UX Modes

All commercial items are stored as a universal `Product` entity in ScyllaDB and rendered dynamically according to `ProductUxMode`:

| UX Mode | Typical Business Domain | Specific Visual & Operational Traits |
|---|---|---|
| `FoodBeverage` | Restaurants, Fast Food, Cafes | Modifiers (extra cheese, sugar level), combo items, kitchen prep routing |
| `Fashion` | Apparel, Shoes, Boutique | Matrix grid (Size x Color), stock levels, fitting room holds |
| `Hotel` | Hospitality, Resorts, Hostels | Check-in/out date range, room types, guest count, keycard status |
| `Cinema` | Theaters, Concerts | Interactive seat selection, showtimes, subtitle/hall specs |
| `Fitness` | Gyms, Crossfit, Swimming Pools | Membership duration, trainer booking, visit limit counters |
| `Medical` | Clinics, Pharmacies, Labs | Doctor consultation slots, prescriptions, batch/expiry dates |
| `VehicleRental`| Car rental, Scooters, Bicycles | Hourly/daily rates, insurance options, fuel/battery level |
| `Electronic` | Gadgets, Hardware | Technical specs table, serial/IMEI tracking, warranty plans |
| *(and 20 others)*| *Floristry, Books, Grocery, etc.* | *Domain-specific metadata JSON schema* |

### Key Components:
- `UniversalProductExperienceRenderer.razor`: Dispatches to specific mode components (`FoodBeverageExperience.razor`, `FashionExperience.razor`, etc.).
- `ProductVariantSelector.razor`: Matrix of variants (sizes, colors, packages).
- Run the audit script to check implementation status of all 28 modes:
  ```powershell
  pwsh .agents/skills/socratic-retail-fintech/scripts/Audit-UxModes.ps1
  ```

---

## 2. 💳 FinTech & Payment Gateways

The payment pipeline spans `Paying.Application` (backend) and `UniversalCheckoutPanel.razor` (frontend):

### Payment Rails:
1. **Uzcard / Humo**: Direct acquiring, card-to-card, SMS OTP verification, and tokenized recurring payments.
2. **Visa / Mastercard / UnionPay**: International acquiring for tourists and cross-border commerce.
3. **Installments (Nasiya)**: 3, 6, 12-month installment schedules with automated risk scoring and merchant payout.
4. **Cash / Cashier POS**: Physical cash drawer handling with mandatory fiscal registration.

---

## 3. 🧾 Fiscalization & OFD Compliance (Uzbekistan Tax Committee / Soliq)

> **CRITICAL RULE**: Every completed commercial order must generate a legally compliant **Fiscal Receipt** registered with a licensed Virtual Cash Register (OFD operator) in Uzbekistan.

### Mandatory Fiscal Fields:
- **TIN / PINFL** of Merchant Organization (`organization_tin`).
- **Fiscal Module Serial Number** (`terminal_id` / `virtual_kassa_id`).
- **IKPU Code** (ИКПУ / Tasnif / MXIK): National product catalog classification code for each order item.
- **VAT Rate & Amount** (НДС / QQS: typically 12% or 0%).
- **Package Code** (Упаковка): Unit of measure code.
- **Fiscal Sign & Fiscal URL**: Generates the verification QR code scanned in the Soliq mobile app.

---

## 4. 💬 Socratic Chat & Real-Time Notifications

- Bi-directional gRPC streaming between customer kiosks/apps and organization staff.
- Audio/visual cues for incoming cashier orders and kitchen display alerts.

---

## 5. 🗺️ Socratic Map & Geospatial Branches (Leaflet / OSM)

- `SocraticMap.razor`: OpenStreetMap / Leaflet integration for displaying organization branch locations, delivery boundaries, and courier tracking.
- Isolated from server prerendering with `@rendermode="new InteractiveAutoRenderMode(prerender: false)"`.
