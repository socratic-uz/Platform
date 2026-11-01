/* ==========================================================================
   Material Web Components - Slider Blazor Interop Patch
   Normalizes md-slider value property to string for Blazor two-way binding.
   ========================================================================== */

export function patchSlider() {
    if (typeof window === 'undefined' || typeof customElements === 'undefined') {
        return;
    }

    customElements.whenDefined('md-slider').then(() => {
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
        }
    });
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

patchSlider();

