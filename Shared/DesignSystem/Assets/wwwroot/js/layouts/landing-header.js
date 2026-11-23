/**
 * Lightweight Static SSR interaction handler for LandingHeader (<header landing>).
 * Only activates when document.documentElement.dataset.ssr === 'true'
 * so InteractiveAuto pages continue using C# event handlers without double-firing.
 */
export function mountLandingHeader(headerEl) {
    if (!headerEl || headerEl._ssrLandingMounted) return;
    if (document.documentElement.dataset.ssr !== 'true') return;
    headerEl._ssrLandingMounted = true;

    const drawerBtn = headerEl.querySelector('[drawer]');
    const getDialog = () => document.querySelector('body > aside:has(~ main) > md-dialog[landing]');

    if (drawerBtn) {
        drawerBtn.addEventListener('click', () => {
            const dlg = getDialog();
            if (!dlg) return;
            if (dlg.hasAttribute('open') || dlg.open) {
                if (typeof dlg.close === 'function') dlg.close();
                else dlg.removeAttribute('open');
            } else {
                if (typeof dlg.show === 'function') dlg.show();
                else dlg.setAttribute('open', '');
            }
        });
    }

    const dlg = getDialog();
    if (dlg) {
        dlg.addEventListener('click', (e) => {
            const target = e.target;
            if (target?.closest('a[href], md-filled-button[href], header[slot="headline"] md-icon-button')) {
                if (typeof dlg.close === 'function') dlg.close();
                else dlg.removeAttribute('open');
            }
        });
    }

    const menus = headerEl.querySelectorAll('nav > menu[dropdown]');
    menus.forEach((menuWrap) => {
        const trigger = menuWrap.querySelector(':scope > md-text-button, :scope > md-outlined-button, :scope > md-filled-button, :scope > button');
        const menu = menuWrap.querySelector(':scope > md-menu');
        if (!trigger || !menu) return;

        let closeTimer = null;
        const cancelClose = () => {
            if (closeTimer) {
                clearTimeout(closeTimer);
                closeTimer = null;
            }
        };

        menuWrap.addEventListener('mouseenter', () => {
            cancelClose();
            menu.open = true;
        });

        menuWrap.addEventListener('mouseleave', () => {
            cancelClose();
            closeTimer = setTimeout(() => {
                menu.open = false;
            }, 120);
        });

        trigger.addEventListener('click', () => {
            cancelClose();
            menu.open = !menu.open;
        });

        menu.addEventListener('click', (e) => {
            if (e.target?.closest('md-menu-item')) {
                menu.open = false;
            }
        });
    });
}
