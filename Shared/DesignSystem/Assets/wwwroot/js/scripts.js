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
import './features/auth/passkey.js';
import './features/auth/telegram.js';
import './features/auth/oauth-popup.js';
import * as ReceiptPrinter from './features/hardware/receipt-printer.js';
import * as CameraStreamerLoader from './features/hardware/camera-streamer-loader.js';
import * as Map from './features/map/map.js';
import * as AdNetwork from './features/ads/ad-network.js';

// 4. Smart Widgets (Chat, QR Designer, Seat Designer, WebRTC Remote Support)
import * as ChatInput from './widgets/chat-input.js';
import * as ChatMessageList from './widgets/chat-message-list.js';
import * as QrStyling from './widgets/qr-styling.js';
import * as SeatDesigner from './widgets/seat-designer.js';
import * as RemoteSupport from './widgets/remote-support-webrtc.js';
import * as Markdown from './widgets/markdown.js';

// 5. Page-Specific Scripts (Landing Orchestration, Home Feed & Media Player)
import * as Home from './pages/home.js';
import * as Landing from './pages/landing.js';
import * as Player from './pages/player.js';

// ==========================================================================
// Centralized Component Lifecycle Registrations (Single Observer)
// ==========================================================================
registerComponent('color', {
    mount: () => ColorTheme.autoMountTheme()
});

registerComponent('figure[map]', {
    mount: (el) => Map.autoMountMap(el),
    unmount: () => Map.destroyMap()
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
// ES Module Named Exports & Re-Exports (Single Module Hub for Blazor Interop)
// ==========================================================================
export * from './layouts/color-theme.js';
export * from './layouts/reconnect-modal.js';
export * from './features/hardware/receipt-printer.js';
export * from './features/hardware/camera-streamer-loader.js';
export * from './features/map/map.js';
export * from './features/ads/ad-network.js';
export * from './widgets/chat-input.js';
export * from './widgets/qr-styling.js';
export * from './widgets/seat-designer.js';
export {
    remoteSupport,
    remoteSupportWebRtc,
    startCall,
    joinCall,
    startHost,
    startViewer,
    handleReceiveShop,
    handleReceiveAnswer,
    handleAddIceCandidate,
    showPointer,
    close
} from './widgets/remote-support-webrtc.js';
export * from './widgets/markdown.js';
export * from './pages/home.js';
export * from './pages/landing.js';
export * from './pages/player.js';

export {
    ColorTheme,
    ReconnectModal,
    MaterialColorUtilities,
    SliderPatch,
    ReceiptPrinter,
    CameraStreamerLoader,
    Map,
    AdNetwork,
    ChatInput,
    ChatMessageList,
    QrStyling,
    SeatDesigner,
    RemoteSupport,
    Markdown,
    Home,
    Landing,
    Player,
    registerComponent
};
