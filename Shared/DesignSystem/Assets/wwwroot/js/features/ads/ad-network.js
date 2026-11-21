/**
 * Socratic Platform - Ad Network & Analytics Integration (GA4, Yandex.Metrika, Google AdSense, Yandex Direct)
 */

window.yaContextCb = window.yaContextCb || [];

if (typeof window !== 'undefined' && /^https?:$/i.test(window.location?.protocol || '')) {
    // Google Analytics 4 (G-3GKEJC8CEW)
    window.dataLayer = window.dataLayer || [];
    window.gtag = window.gtag || function () { window.dataLayer.push(arguments); };
    window.gtag('js', new Date());
    window.gtag('config', 'G-3GKEJC8CEW');

    // Yandex.Metrika (114253340)
    window.ym = window.ym || function () { (window.ym.a = window.ym.a || []).push(arguments); };
    window.ym.l = 1 * new Date();
    window.ym(114253340, 'init', {
        ssr: true,
        webvisor: true,
        clickmap: true,
        ecommerce: 'dataLayer',
        referrer: document.referrer,
        url: location.href,
        accurateTrackBounce: true,
        trackLinks: true
    });
}

export const adNetwork = {
    pushGoogle() {
        try {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
            this.observeUnfilled();
        } catch (_) {
            this.observeUnfilled();
        }
    },
    observeUnfilled() {
        try {
            const checkSlot = (ins) => {
                const status = ins.getAttribute('data-ad-status');
                if (status !== 'unfilled') return;

                const wrapper = ins.closest('aside[ads][placement]') || ins.closest('aside[ads]');
                if (!wrapper || wrapper.dataset.fallbackHandled === 'true') return;
                wrapper.dataset.fallbackHandled = 'true';

                const fb = wrapper.querySelector('[data-yandex-fallback]');
                const blockId = fb?.getAttribute('data-yandex-fallback');
                if (fb && blockId && fb.id) {
                    fb.hidden = false;
                    this.renderYandex(blockId, fb.id, () => {
                        wrapper.setAttribute('data-ad-empty', 'true');
                    });
                } else {
                    wrapper.setAttribute('data-ad-empty', 'true');
                }
            };

            document.querySelectorAll('ins.adsbygoogle').forEach((ins) => {
                if (ins.getAttribute('data-ad-status') === 'unfilled') {
                    checkSlot(ins);
                } else if (!ins.dataset.adObserved) {
                    ins.dataset.adObserved = 'true';
                    const obs = new MutationObserver(() => checkSlot(ins));
                    obs.observe(ins, { attributes: true, attributeFilter: ['data-ad-status'] });
                }
            });
        } catch (_) {}
    },
    renderYandex(blockId, containerId, onUnfilled) {
        try {
            const render = () => {
                if (window.Ya?.Context?.AdvManager) {
                    window.Ya.Context.AdvManager.render({
                        blockId,
                        renderTo: containerId,
                        onError: () => {
                            if (typeof onUnfilled === 'function') onUnfilled();
                        }
                    });
                    setTimeout(() => {
                        const el = document.getElementById(containerId);
                        if (el && el.clientHeight < 10 && typeof onUnfilled === 'function') {
                            onUnfilled();
                        }
                    }, 3000);
                } else if (typeof onUnfilled === 'function') {
                    onUnfilled();
                }
            };
            if (window.Ya?.Context?.AdvManager) {
                render();
            } else {
                window.yaContextCb.push(render);
            }
        } catch (_) {
            if (typeof onUnfilled === 'function') onUnfilled();
        }
    }
};

window.adNetwork = adNetwork;
window.adSense = {
    push() {
        adNetwork.pushGoogle();
    }
};

