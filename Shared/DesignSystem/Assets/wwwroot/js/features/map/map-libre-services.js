import { state } from './map-libre-state.js';

export function startLiveLocationWatch(dotNetRef, enableHighAccuracy = true) {
    if (!navigator?.geolocation || !dotNetRef) {
        return false;
    }

    state.activeDotNetRef = dotNetRef;

    if (state.liveLocationWatchId !== null) {
        navigator.geolocation.clearWatch(state.liveLocationWatchId);
        state.liveLocationWatchId = null;
    }

    state.liveLocationWatchId = navigator.geolocation.watchPosition(
        (position) => {
            const heading = Number.isFinite(position.coords.heading) ? Number(position.coords.heading) : -1;
            const speed = Number.isFinite(position.coords.speed) ? Number(position.coords.speed) : -1;
            dotNetRef
                .invokeMethodAsync(
                    "OnLiveLocationUpdate",
                    Number(position.coords.latitude),
                    Number(position.coords.longitude),
                    Number(position.coords.accuracy),
                    heading,
                    speed
                )
                .catch(() => {
                    // Ignore detached circuit errors.
                });
        },
        () => {
            // Ignore temporary watch errors.
        },
        {
            enableHighAccuracy,
            timeout: 15000,
            maximumAge: 1000
        }
    );

    return true;
}

export function stopLiveLocationWatch() {
    if (!navigator?.geolocation || state.liveLocationWatchId === null) {
        return;
    }

    navigator.geolocation.clearWatch(state.liveLocationWatchId);
    state.liveLocationWatchId = null;
}

export function speakText(text, lang = "ru-RU") {
    try {
        if (!window.speechSynthesis || !text) {
            return false;
        }

        const utterance = new SpeechSynthesisUtterance(String(text));
        utterance.lang = lang;
        utterance.rate = 1;
        utterance.pitch = 1;
        window.speechSynthesis.cancel();
        window.speechSynthesis.speak(utterance);
        return true;
    } catch {
        return false;
    }
}

export async function tryGetCurrentPosition(timeoutMs = 8000) {
    if (!navigator?.geolocation) {
        return { success: false, latitude: 0, longitude: 0 };
    }

    try {
        const position = await new Promise((resolve, reject) => {
            navigator.geolocation.getCurrentPosition(resolve, reject, {
                enableHighAccuracy: true,
                timeout: timeoutMs,
                maximumAge: 0
            });
        });

        return {
            success: true,
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            accuracy: position.coords.accuracy
        };
    } catch {
        return { success: false, latitude: 0, longitude: 0 };
    }
}

export function registerDotNetRef(dotNetRef) {
    state.activeDotNetRef = dotNetRef;
}

export function initBottomSheetSwipe() {
    const panel = document.querySelector(".organizations-map-controls");
    if (!panel) return;

    const handle = panel.querySelector(".map-panel-toggle-tab") || panel;

    let startY = 0;
    let startHeight = 0;
    let isDragging = false;

    handle.addEventListener("touchstart", (e) => {
        if (window.innerWidth > 680) return; // Only mobile
        startY = e.touches[0].clientY;
        startHeight = panel.offsetHeight;
        isDragging = true;
        panel.style.transition = "none"; // Disable CSS transitions during drag
    }, { passive: true });

    handle.addEventListener("touchmove", (e) => {
        if (!isDragging) return;
        const currentY = e.touches[0].clientY;
        const deltaY = startY - currentY; // Dragging up increases height
        const newHeight = startHeight + deltaY;

        const maxHeightLimit = window.innerHeight * 0.9;
        if (newHeight >= 110 && newHeight <= maxHeightLimit + 50) {
            panel.style.maxHeight = `${newHeight}px`;
        }
    }, { passive: true });

    handle.addEventListener("touchend", (e) => {
        if (!isDragging) return;
        isDragging = false;
        panel.style.transition = ""; // Restore CSS transitions

        const currentHeight = panel.offsetHeight;
        
        let collapsedHeight = 130;
        if (panel.classList.contains("details-mode")) {
            collapsedHeight = 205;
        } else if (panel.classList.contains("yandex-route-mode")) {
            collapsedHeight = 185;
        }

        const expandedHeight = window.innerHeight * 0.9;
        const midPoint = (collapsedHeight + expandedHeight) / 2;

        if (currentHeight > midPoint) {
            panel.style.maxHeight = "";
            if (state.activeDotNetRef?.invokeMethodAsync) {
                state.activeDotNetRef.invokeMethodAsync("SetControlsPanelExpanded", true).catch(() => {});
            }
        } else {
            panel.style.maxHeight = "";
            if (state.activeDotNetRef?.invokeMethodAsync) {
                state.activeDotNetRef.invokeMethodAsync("SetControlsPanelExpanded", false).catch(() => {});
            }
        }
    });
}
