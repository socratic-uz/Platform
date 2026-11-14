/**
 * Socratic Platform - Ad Network Integration (Google AdSense & Yandex Direct)
 */

window.yaContextCb = window.yaContextCb || [];

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

window.socraticAdNetwork = adNetwork;
window.socraticAdSense = {
    push() {
        adNetwork.pushGoogle();
    }
};

