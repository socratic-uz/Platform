# Socratic Fiscalization, OFD & POS Architecture Rules

## 1. Legal Compliance & Fiscalization in Uzbekistan (Soliq / OFD)
Every commercial sale registered in Socratic (Kiosk, Cashier Terminal, Online Delivery) must comply with the State Tax Committee of the Republic of Uzbekistan (ГНК РУз / Soliq):

- **Virtual Cash Register (Виртуальная касса / OFD)**:
  - All orders finalized with cash, terminal card, or online payment must generate an electronic fiscal receipt.
  - Required fields per receipt item:
    - **IKPU Code (ИКПУ / Tasnif)**: 17-digit national product classification code.
    - **Package Code (Упаковка)**: Unit of measurement code (e.g. pieces, kg, liters).
    - **VAT Rate (НДС / QQS)**: Currently 12% or 0% (exempt).
    - **Organization TIN / PINFL**: Merchant tax identification.
    - **Terminal ID / Serial Number**: Registered hardware or software terminal identifier.
  - The fiscal operator generates a **Fiscal Sign** (`fiscal_sign`) and **Fiscal URL** (`fiscal_url`) encoded into a verifiable QR code.

## 2. Thermal Receipt Printing (ESC/POS)
- POS printers (58mm / 80mm) consume raw ESC/POS byte commands over TCP port 9100 or USB Virtual COM.
- The fiscal QR code must be printed at the bottom of the paper receipt with standard ECC Level M.
- Never rely on browser `window.print()` for unattended POS or Kiosk thermal receipt printing.

## 3. Peripheral Isolation
- Cash drawer kick pulses (`0x1B, 0x70, 0x00, 0x19, 0xFA`) must execute asynchronously without blocking cashier UI interaction.
- Barcode scanners operating in Keyboard Wedge mode must buffer input and filter rapid keystrokes to differentiate hardware scanner scans from manual keyboard typing.
