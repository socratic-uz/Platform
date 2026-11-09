/* ==========================================================================
   Socratic Platform - Master JavaScript Hub
   Component-Driven 7-Layer Architecture (Mirroring styles.css)
   ========================================================================== */

import { registerComponent } from './lifecycle.js';

// 1. Components (Material Web Components & Dynamic Color Utilities)
import './components/material-web.bundle.js';
import * as MaterialColorUtilities from './components/material-color-utilities.bundle.js';
import * as SliderPatch from './components/slider-patch.js';
import * as DialogPatch from './components/dialog-patch.js';

// 2. Layouts (Color Theme & Reconnect Modal)
import * as ColorTheme from './layouts/color-theme.js';
import * as ReconnectModal from './layouts/reconnect-modal.js';

// 3. Features (Auth, Hardware Peripherals, Spatial Map & Ads)
import './features/auth/passkeyHelper.js';
import './features/auth/telegramHelper.js';
import './features/auth/oauthPopupHelper.js';
import * as ReceiptPrinterHelper from './features/hardware/receipt-printer-helper.js';
import * as CameraStreamerLoader from './features/hardware/camera-streamer-loader.js';
import * as SocraticMap from './features/map/socratic-map.js';
import * as AdNetwork from './features/ads/ad-network.js';

// 4. Smart Widgets (Chat, QR Designer, Seat Designer, WebRTC Remote Support)
import * as ChatInput from './widgets/chat-input.js';
import * as ChatMessageList from './widgets/chat-message-list.js';
import * as QrStylingHelper from './widgets/qr-styling-helper.js';
import * as SeatDesigner from './widgets/seat-designer.js';
import * as RemoteSupport from './widgets/remoteSupportWebRtc.js';
import * as Markdown from './widgets/markdown.js';

// 5. Page-Specific Scripts (Landing Orchestration, Home Feed & Media Player)
import * as Home from './pages/home.js';
import * as Landing from './pages/landing.js';
import * as Player from './pages/player.js';

// ==========================================================================
// Unified Global Socratic Namespace for Browser & Blazor Interop
// ==========================================================================
window.Socratic = window.Socratic || {};
window.Socratic.ColorTheme = ColorTheme;
window.Socratic.ReconnectModal = ReconnectModal;
window.Socratic.MaterialColorUtilities = MaterialColorUtilities;
window.Socratic.SliderPatch = SliderPatch;
window.Socratic.ReceiptPrinter = ReceiptPrinterHelper;
window.Socratic.CameraStreamer = CameraStreamerLoader;
window.Socratic.Map = SocraticMap;
window.Socratic.AdNetwork = AdNetwork;
window.Socratic.Widgets = {
    ChatInput,
    ChatMessageList,
    QrStyling: QrStylingHelper,
    SeatDesigner,
    RemoteSupport,
    Markdown
};
window.Socratic.Pages = {
    Home,
    Landing,
    Player,
    registerComponent
};
window.Socratic.Lifecycle = {
    registerComponent
};

// ==========================================================================
// Centralized Component Lifecycle Registrations (Single Observer)
// ==========================================================================
registerComponent('color', {
    mount: () => ColorTheme.autoMountTheme()
});

registerComponent('figure[map]', {
    mount: (el) => SocraticMap.autoMountMap(el),
    unmount: () => SocraticMap.destroyMap()
});

registerComponent('main[player]', {
    mount: () => Player.autoMountPlayer()
});

registerComponent('main[landing]', {
    mount: () => Landing.autoMountLanding(),
    unmount: () => Landing.disposeLandingAnimations()
});

registerComponent('figure[seat-designer], .seat-designer-canvas-container', {
    mount: (el) => SeatDesigner.autoMountSeatDesigner(el),
    unmount: (el) => el.id && SeatDesigner.dispose(el.id)
});

registerComponent('textarea[chat-input], .chat-input textarea, main[chat] textarea', {
    mount: (el) => ChatInput.init(el)
});

registerComponent('pre.snippet, .snippet, pre code, pre.mermaid, .mermaid', {
    mount: () => Markdown.autoMountMarkdown()
});

// ==========================================================================
// ES Module Named Exports
// ==========================================================================
export {
    ColorTheme,
    ReconnectModal,
    MaterialColorUtilities,
    SliderPatch,
    ReceiptPrinterHelper,
    CameraStreamerLoader,
    SocraticMap,
    AdNetwork,
    ChatInput,
    ChatMessageList,
    QrStylingHelper,
    SeatDesigner,
    RemoteSupport,
    Markdown,
    Home,
    Landing,
    Player,
    registerComponent
};

// ==========================================================================
// Centralized Component Lifecycle Registrations (Single Observer)
// ==========================================================================
registerComponent('color', {
    mount: () => ColorTheme.autoMountTheme()
});

registerComponent('figure[map]', {
    mount: (el) => SocraticMap.autoMountMap(el),
    unmount: () => SocraticMap.destroyMap()
});

registerComponent('main[player]', {
    mount: () => Player.autoMountPlayer()
});

registerComponent('main[landing]', {
    mount: () => Landing.autoMountLanding(),
    unmount: () => Landing.disposeLandingAnimations()
});

registerComponent('figure[seat-designer], .seat-designer-canvas-container', {
    mount: (el) => SeatDesigner.autoMountSeatDesigner(el),
    unmount: (el) => el.id && SeatDesigner.dispose(el.id)
});

registerComponent('textarea[chat-input], .chat-input textarea, main[chat] textarea', {
    mount: (el) => ChatInput.init(el)
});

registerComponent('pre.snippet, .snippet, pre code, pre.mermaid, .mermaid', {
    mount: () => Markdown.autoMountMarkdown()
});
