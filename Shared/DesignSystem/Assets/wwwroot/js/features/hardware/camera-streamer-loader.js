export function loadScript() {
    if (window.JSInterop?.Scripts?.CameraStreamerInterop) {
        return Promise.resolve();
    }
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = '_content/Assets/js/features/hardware/CameraStreamer.js';
        script.onload = resolve;
        script.onerror = reject;
        document.body.appendChild(script);
    });
}
