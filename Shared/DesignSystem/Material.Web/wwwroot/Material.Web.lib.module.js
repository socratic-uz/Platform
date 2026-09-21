export function beforeStart(options, extensions) {
    loadAssets();
}

export function beforeWebAssemblyStart(options, extensions) {
    loadAssets();
}

function loadAssets() {
    if (!document.querySelector('script[src*="material-web.bundle.js"]')) {
        const script = document.createElement('script');
        script.src = '_content/Material.Web/material-web.bundle.js';
        document.body.appendChild(script);
    }
    if (!document.querySelector('script[src*="material-color-utilities.bundle.js"]')) {
        const colorScript = document.createElement('script');
        colorScript.src = '_content/Material.Web/material-color-utilities.bundle.js';
        colorScript.type = 'module';
        document.body.appendChild(colorScript);
    }
    patchSlider();
}

function patchSlider() {
    const sliderClass = customElements.get('md-slider');
    if (sliderClass) {
        const proto = sliderClass.prototype;
        const descriptor = getPropertyDescriptor(proto, 'value');
        if (descriptor && !proto.__patched) {
            proto.__patched = true;
            Object.defineProperty(proto, 'value', {
                get() {
                    const val = descriptor.get.call(this);
                    return val !== undefined && val !== null ? String(val) : val;
                },
                set(val) {
                    descriptor.set.call(this, val === '' ? 0 : Number(val));
                },
                configurable: true,
                enumerable: true
            });
            console.log("[Material.Web] Patched md-slider value property to return string.");
        }
    } else {
        setTimeout(patchSlider, 50);
    }
}

function getPropertyDescriptor(obj, prop) {
    let desc;
    while (obj) {
        desc = Object.getOwnPropertyDescriptor(obj, prop);
        if (desc) {
            return desc;
        }
        obj = Object.getPrototypeOf(obj);
    }
    return undefined;
}
