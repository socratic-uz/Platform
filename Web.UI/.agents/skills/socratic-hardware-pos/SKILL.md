---
name: socratic-hardware-pos
description: Hardware, IoT, and POS integration guide for Socratic cashier terminals, kiosks, and venue designers. USE FOR: generating ESC/POS thermal printing bytes, cash drawer kick pulses, WebHID/WebSerial barcode scanner integrations, Face ID camera biometrics streaming, WebRTC remote screen control, interactive seat/hall layout designers, and thermal receipt template generation. DO NOT USE FOR: standard browser CSS printing or pure database operations.
---

# Socratic Hardware & POS: Peripherals, Biometrics, WebRTC & Seat Designer

Socratic communicates with physical devices connected to cashier terminals (`Terminal.razor`), self-service kiosks (`Kiosk.razor`), and venue managers (`SeatDesigner.razor`).

---

## 1. 🖨️ POS Peripherals: ESC/POS Thermal Printing & Cash Drawers

ESC/POS commands are sent as byte streams over TCP (port 9100) or USB Virtual COM port:

```csharp
public static class EscPosCommands
{
    public static readonly byte[] Initialize = [0x1B, 0x40];
    public static readonly byte[] AlignCenter = [0x1B, 0x61, 0x01];
    public static readonly byte[] AlignLeft = [0x1B, 0x61, 0x00];
    public static readonly byte[] BoldOn = [0x1B, 0x45, 0x01];
    public static readonly byte[] BoldOff = [0x1B, 0x45, 0x00];
    public static readonly byte[] CutPaper = [0x1D, 0x56, 0x41, 0x10];
    
    // Kick cash drawer 1 (pin 2, 25ms pulse)
    public static readonly byte[] OpenDrawer = [0x1B, 0x70, 0x00, 0x19, 0xFA];
}
```

### Barcode & QR Scanners:
- **Keyboard Wedge**: USB scanners emulate keyboard typing terminated with `Enter`. Buffer characters on `@onkeydown` and submit on `Enter`.
- **WebSerial / WebHID**: For direct hardware interrogation in Chromium browsers without OS desktop drivers.

---

## 2. 📷 Camera Streamer & Face ID Biometrics

Biometrics processing connects the camera stream (`CameraStreamer.razor`) to face detection pipelines:
- Captures frame via `canvas.toBlob('image/jpeg', 0.85)`.
- Sends frame to backend via gRPC bidirectional stream for ArcFace feature extraction.
- Stores vector in Qdrant (512 dimensions, cosine similarity threshold $\ge 0.72$).

```razor
<!-- Isolate camera from server prerendering -->
<CameraStreamer @rendermode="new InteractiveServerRenderMode(prerender: false)" 
                OnFaceCaptured="HandleFaceCaptured" />
```

---

## 3. 🎟️ Thermal Receipt & QR Code Designers

- **Receipt Formatting**: Receipts must fit standard 58mm (32 chars) or 80mm (48 chars) thermal paper rolls.
- **Fiscal QR Codes**: Printed at bottom with Uzbek Tax Committee (Soliq) verification URL.
- **Graphic Printing**: Monochrome 1-bit bitmap dithering via ESC/POS bit-image command `GS v 0`.

---

## 4. 🪑 Seat Designer (Cinema, Stadiums, Restaurants)

- Interactive SVG and HTML5 Canvas 2D seat map designer.
- Manages rows, tables, sectors, seat states (`Available`, `Selected`, `Reserved`, `Occupied`, `VIP`).
- Real-time seat locking using gRPC streaming subscriptions to prevent double-booking.

---

## 5. 📡 WebRTC Remote Control & Screen Mirroring

- Enables remote kiosk monitoring, remote assistance, and manager override.
- Transmits WebRTC video stream from kiosk to manager dashboard via STUN/TURN signaling.
