// Set up event handlers safely for reconnect modal
export function initReconnectModal() {
    const reconnectModal = document.getElementById("components-reconnect-modal");
    if (!reconnectModal || reconnectModal.dataset.reconnectBound === "true") return;
    reconnectModal.dataset.reconnectBound = "true";

    reconnectModal.addEventListener("components-reconnect-state-changed", (event) => {
        handleReconnectStateChanged(event, reconnectModal);
    });

    const retryButton = document.getElementById("components-reconnect-button");
    if (retryButton) {
        retryButton.addEventListener("click", () => retry(reconnectModal));
    }

    const resumeButton = document.getElementById("components-resume-button");
    if (resumeButton) {
        resumeButton.addEventListener("click", resume);
    }
}

function handleReconnectStateChanged(event, reconnectModal) {
    if (event.detail.state === "show") {
        reconnectModal.showModal();
    } else if (event.detail.state === "hide") {
        reconnectModal.close();
    } else if (event.detail.state === "failed") {
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    } else if (event.detail.state === "rejected") {
        location.reload();
    }
}

async function retry(reconnectModal) {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);

    try {
        const successful = await Blazor.reconnect();
        if (!successful) {
            const resumeSuccessful = await Blazor.resumeCircuit();
            if (!resumeSuccessful) {
                location.reload();
            } else if (reconnectModal) {
                reconnectModal.close();
            }
        }
    } catch {
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume() {
    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        }
    } catch {
        location.reload();
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        const modal = document.getElementById("components-reconnect-modal");
        await retry(modal);
    }
}

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initReconnectModal);
} else {
    initReconnectModal();
}
