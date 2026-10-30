/**
 * Socratic Platform - Ad Network Integration (Google AdSense & Yandex Direct)
 */

window.yaContextCb = window.yaContextCb || [];

export const adNetwork = {
    pushGoogle() {
        try {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
        } catch (_) {}
    },
    renderYandex(blockId, containerId) {
        try {
            const render = () => {
                if (window.Ya?.Context?.AdvManager) {
                    window.Ya.Context.AdvManager.render({
                        blockId,
                        renderTo: containerId
                    });
                }
            };
            if (window.Ya?.Context?.AdvManager) {
                render();
            } else {
                window.yaContextCb.push(render);
            }
        } catch (_) {}
    }
};

window.socraticAdNetwork = adNetwork;
window.socraticAdSense = {
    push() {
        adNetwork.pushGoogle();
    }
};

