/* ==========================================================================
   Material Web Components - Aside Drawer & Mobile Bottom Sheet Patch
   Transforms body > aside > md-dialog into:
   - Desktop (>= 840px): Side Sheet (Docked [docked] or Floating :not([docked])) — NO drag handle
   - Mobile (< 840px): Bottom Sheet with Drag Handle (Docked [docked] or Floating :not([docked]))
     with 3 Snap Points (peek -> half 48dvh -> full 85dvh)
   ========================================================================== */

export function patchDialog() {
    if (typeof window === 'undefined' || typeof customElements === 'undefined') {
        return;
    }

    function isMobileViewport() {
        return window.innerWidth < 840;
    }

    function isBodyAsideDialog(instance) {
        return Boolean(
            instance &&
            instance.parentElement &&
            instance.parentElement.tagName === 'ASIDE' &&
            instance.parentElement.parentElement === document.body
        );
    }

    function getAsideNavHeight(aside) {
        if (!aside) return 0;
        const nav = aside.querySelector(':scope > nav');
        return nav ? 60 : 0;
    }

    function isMobileBottomSheet(instance) {
        return (
            isMobileViewport() &&
            isBodyAsideDialog(instance) &&
            getAsideNavHeight(instance.parentElement) > 0
        );
    }

    function isPermanentSheet(instance) {
        return (
            instance.hasAttribute('docked') ||
            instance.hasAttribute('map-controls')
        );
    }

    function syncHandleVisibility(instance) {
        if (!isBodyAsideDialog(instance)) return;
        const bottomMode = isMobileBottomSheet(instance);
        instance.setAttribute('data-sheet-mode', bottomMode ? 'bottom' : 'side');
        const shadow = instance && instance.shadowRoot;
        if (!shadow) return;
        const handle = shadow.querySelector('.sheet-drag-handle');
        if (handle) {
            handle.hidden = !bottomMode;
        }
    }

    function ensureSheetScrim(instance, snap) {
        if (typeof document === 'undefined') return;
        if (!isBodyAsideDialog(instance)) return;
        const aside = instance.parentElement;

        const isOpen = instance.hasAttribute('open');
        const needsScrim =
            isMobileViewport() &&
            isOpen &&
            (snap === 'full' || !isPermanentSheet(instance));

        let scrim = document.getElementById('socratic-sheet-scrim');
        if (!needsScrim) {
            if (scrim) {
                scrim.style.opacity = '0';
                scrim.style.pointerEvents = 'none';
            }
            return;
        }

        const navH = getAsideNavHeight(aside);
        if (!scrim) {
            scrim = document.createElement('div');
            scrim.id = 'socratic-sheet-scrim';
            Object.assign(scrim.style, {
                position: 'fixed',
                inset: `0 0 ${navH}px 0`,
                background: 'rgba(0, 0, 0, 0.32)',
                zIndex: '998',
                opacity: '0',
                pointerEvents: 'none',
                transition: 'opacity 200ms ease'
            });
            document.body.appendChild(scrim);
        } else {
            scrim.style.inset = `0 0 ${navH}px 0`;
        }

        scrim.onclick = () => {
            if (isPermanentSheet(instance)) {
                setSheetSnap(instance, 'peek');
            } else if (typeof instance.close === 'function') {
                instance.close();
            }
        };

        requestAnimationFrame(() => {
            if (scrim) {
                scrim.style.opacity = '1';
                scrim.style.pointerEvents = 'auto';
            }
        });
    }

    function setSheetSnap(instance, nextSnap) {
        const aside = isBodyAsideDialog(instance) ? instance.parentElement : null;
        const currentSnap = instance.getAttribute('data-sheet-snap') || 'peek';

        if (
            aside &&
            instance.hasAttribute('docked') &&
            currentSnap === 'peek' &&
            nextSnap !== 'peek' &&
            isMobileViewport()
        ) {
            const rect = instance.getBoundingClientRect();
            if (rect.height > 0) {
                aside.style.setProperty('--sheet-peek-h', `${Math.round(rect.height)}px`);
            }
        }

        instance.setAttribute('data-sheet-snap', nextSnap);
        ensureSheetScrim(instance, nextSnap);
    }

    function setupBottomSheetGestures(instance) {
        if (!instance.__sheetGesturesAttached) {
            instance.__sheetGesturesAttached = true;

            const initialSnap = isPermanentSheet(instance)
                ? (instance.hasAttribute('expanded') ? 'half' : 'peek')
                : 'half';
            instance.setAttribute('data-sheet-snap', initialSnap);

            // Observe 'expanded' and 'open' attribute changes from Blazor
            const attrObserver = new MutationObserver((mutations) => {
                for (const m of mutations) {
                    if (m.attributeName === 'expanded') {
                        if (instance.hasAttribute('expanded')) {
                            if (instance.getAttribute('data-sheet-snap') === 'peek') {
                                setSheetSnap(instance, 'half');
                            }
                        } else if (isPermanentSheet(instance)) {
                            setSheetSnap(instance, 'peek');
                        }
                    } else if (m.attributeName === 'open') {
                        syncHandleVisibility(instance);
                        ensureSheetScrim(instance, instance.getAttribute('data-sheet-snap') || 'peek');
                    }
                }
            });
            attrObserver.observe(instance, { attributes: true, attributeFilter: ['expanded', 'open'] });

            let startY = 0;
            let startHeight = 0;
            let startTime = 0;
            let isDragging = false;
            let activePointerId = null;

            function canStartDragFromTarget(target, isHandle) {
                if (isHandle) return true;
                if (!target || !(target instanceof Element)) return false;
                if (
                    target.closest(
                        'button, a, input, select, textarea, md-icon-button, md-filled-icon-button, md-filled-tonal-icon-button, md-outlined-icon-button, md-filled-button, md-outlined-button, md-text-button, md-tonal-button, md-filled-text-field, md-outlined-text-field, md-tabs, md-primary-tab, md-secondary-tab, md-chip-set, md-filter-chip'
                    )
                ) {
                    return false;
                }
                return Boolean(target.closest('header[slot="headline"], [slot="headline"], search[search-mode] > header'));
            }

            instance.__onSheetPointerDown = function onPointerDown(e, isHandle) {
                if (!isMobileBottomSheet(instance)) return;
                if (e.button !== undefined && e.button !== 0) return;
                if (!canStartDragFromTarget(e.target, isHandle)) return;

                activePointerId = e.pointerId;
                startY = e.clientY;
                startTime = performance.now();
                const rect = instance.getBoundingClientRect();
                startHeight = rect.height;
                isDragging = false;

                const aside = instance.parentElement;
                if (
                    aside &&
                    instance.hasAttribute('docked') &&
                    (instance.getAttribute('data-sheet-snap') || 'peek') === 'peek' &&
                    rect.height > 0
                ) {
                    aside.style.setProperty('--sheet-peek-h', `${Math.round(rect.height)}px`);
                }

                window.addEventListener('pointermove', onPointerMove, { passive: false });
                window.addEventListener('pointerup', onPointerUp);
                window.addEventListener('pointercancel', onPointerUp);
            };

            function onPointerMove(e) {
                if (e.pointerId !== activePointerId) return;
                const dy = startY - e.clientY;

                if (!isDragging && Math.abs(dy) > 6) {
                    isDragging = true;
                    instance.setAttribute('data-sheet-dragging', '');
                }

                if (isDragging) {
                    if (e.cancelable) e.preventDefault();
                    const navH = getAsideNavHeight(instance.parentElement);
                    const maxH = Math.round(window.innerHeight * 0.85 - navH);
                    const minH = 56;
                    const nextH = Math.max(minH, Math.min(maxH, Math.round(startHeight + dy)));
                    instance.style.setProperty('--sheet-drag-h', `${nextH}px`);
                }
            }

            function onPointerUp(e) {
                if (e.pointerId !== activePointerId) return;
                window.removeEventListener('pointermove', onPointerMove);
                window.removeEventListener('pointerup', onPointerUp);
                window.removeEventListener('pointercancel', onPointerUp);
                activePointerId = null;

                const dy = startY - e.clientY;
                const dt = Math.max(1, performance.now() - startTime);
                const velocityY = dy / dt; // px/ms (> 0 = swipe up, < 0 = swipe down)
                const currentSnap = instance.getAttribute('data-sheet-snap') || 'peek';

                if (!isDragging) {
                    if (Math.abs(dy) <= 6) {
                        setSheetSnap(instance, currentSnap === 'peek' ? 'half' : 'peek');
                    }
                    return;
                }

                const finalHeight = instance.getBoundingClientRect().height;
                instance.removeAttribute('data-sheet-dragging');
                instance.style.removeProperty('--sheet-drag-h');

                const vh = window.innerHeight;
                const permanent = isPermanentSheet(instance);

                if (velocityY > 0.45) {
                    setSheetSnap(instance, currentSnap === 'peek' ? 'half' : 'full');
                    return;
                }

                if (velocityY < -0.45) {
                    if (currentSnap === 'full') {
                        setSheetSnap(instance, 'half');
                    } else if (permanent) {
                        setSheetSnap(instance, 'peek');
                    } else if (typeof instance.close === 'function') {
                        instance.close();
                    }
                    return;
                }

                const ratio = finalHeight / vh;
                if (ratio >= 0.62) {
                    setSheetSnap(instance, 'full');
                } else if (ratio >= 0.25) {
                    setSheetSnap(instance, 'half');
                } else if (permanent) {
                    setSheetSnap(instance, 'peek');
                } else if (typeof instance.close === 'function') {
                    instance.close();
                }
            }

            instance.addEventListener('pointerdown', (e) => instance.__onSheetPointerDown(e, false));
            instance.addEventListener('click', (e) => {
                if (!isMobileViewport() || !isPermanentSheet(instance)) return;
                const target = e.target;
                if (target instanceof Element && target.closest('nav[slot="content"] md-list-item, nav[slot="content"] a')) {
                    setSheetSnap(instance, 'peek');
                }
            });
        }

        const shadow = instance.shadowRoot;
        if (shadow) {
            const handle = shadow.querySelector('.sheet-drag-handle');
            if (handle && !handle.__listenerAttached && instance.__onSheetPointerDown) {
                handle.__listenerAttached = true;
                handle.addEventListener('pointerdown', (e) => {
                    e.stopPropagation();
                    instance.__onSheetPointerDown(e, true);
                });
            }
        }
    }

    function applyPatch(dialogClass) {
        if (!dialogClass || dialogClass.prototype.__asidePatchedV4) {
            return;
        }

        const proto = dialogClass.prototype;
        proto.__asidePatched = true;
        proto.__asidePatchedV2 = true;
        proto.__asidePatchedV3 = true;
        proto.__asidePatchedV4 = true;

        const origShow = proto.show;
        const origClose = proto.close;
        const origFirstUpdated = proto.firstUpdated;

        function ensureAsideDrawerLayout(instance) {
            if (!isBodyAsideDialog(instance)) {
                return false;
            }

            // Ensure any unslotted direct children are assigned to slot="content"
            for (let i = 0; i < instance.children.length; i++) {
                const child = instance.children[i];
                if (!child.hasAttribute('slot')) {
                    child.setAttribute('slot', 'content');
                }
            }

            const shadow = instance.shadowRoot;
            if (shadow && !shadow.querySelector('style[data-socratic-aside-sheet="v4"]')) {
                const style = document.createElement('style');
                style.setAttribute('data-socratic-aside-sheet', 'v4');
                style.textContent = `
                    :host {
                        display: none;
                    }
                    :host([open]) {
                        display: grid !important;
                    }
                    .scrim {
                        display: none !important;
                    }
                    dialog {
                        position: relative !important;
                        inset: auto !important;
                        width: 100% !important;
                        height: 100% !important;
                        max-width: 100% !important;
                        max-height: 100% !important;
                        min-width: 0 !important;
                        min-height: 0 !important;
                        margin: 0 !important;
                        display: flex !important;
                        flex-direction: column !important;
                        overflow: hidden !important;
                    }
                    .container {
                        width: 100% !important;
                        height: 100% !important;
                        flex: 1 1 auto !important;
                        display: flex !important;
                        flex-direction: column !important;
                        overflow: hidden !important;
                    }
                    .sheet-drag-handle,
                    .sheet-drag-handle > span {
                        display: none !important;
                    }
                    @media screen and (max-width: 839.98px) {
                        :host([data-sheet-mode="bottom"]) .sheet-drag-handle {
                            position: relative !important;
                            z-index: 2 !important;
                            display: grid !important;
                            place-items: center !important;
                            width: 100% !important;
                            height: 22px !important;
                            padding-top: 6px !important;
                            box-sizing: border-box !important;
                            cursor: grab !important;
                            touch-action: none !important;
                            user-select: none !important;
                            flex-shrink: 0 !important;
                        }
                        :host([data-sheet-mode="bottom"]) .sheet-drag-handle > span {
                            display: block !important;
                            width: 36px !important;
                            height: 4px !important;
                            border-radius: 999px !important;
                            background: var(--md-sys-color-on-surface-variant, #79747e) !important;
                            opacity: 0.55 !important;
                            transition: opacity 0.15s ease, transform 0.15s ease !important;
                        }
                        :host([data-sheet-mode="bottom"]) .sheet-drag-handle:active > span {
                            opacity: 0.9 !important;
                            transform: scaleX(1.18) !important;
                        }
                    }
                    .scroller {
                        flex: 1 1 auto !important;
                        min-height: 0 !important;
                        overflow: hidden !important;
                        display: flex !important;
                        flex-direction: column !important;
                    }
                    .content {
                        flex: 1 1 auto !important;
                        height: 100% !important;
                        min-height: 0 !important;
                        overflow: hidden !important;
                        display: grid !important;
                        grid-template-rows: minmax(0, 1fr) !important;
                    }
                    slot[name=content]::slotted(*) {
                        padding: 16px !important;
                        min-height: 0 !important;
                        max-height: 100% !important;
                        overflow-y: auto !important;
                        box-sizing: border-box !important;
                    }
                    slot[name=headline]::slotted(*) {
                        padding: 12px 16px !important;
                    }
                `;
                shadow.appendChild(style);
            }

            if (shadow) {
                const container = shadow.querySelector('.container');
                if (container && !shadow.querySelector('.sheet-drag-handle')) {
                    const handle = document.createElement('div');
                    handle.className = 'sheet-drag-handle';
                    handle.setAttribute('part', 'drag-handle');
                    handle.setAttribute('aria-hidden', 'true');
                    handle.hidden = !isMobileBottomSheet(instance);
                    handle.innerHTML = '<span></span>';
                    container.insertBefore(handle, container.firstChild);
                }
            }

            syncHandleVisibility(instance);
            setupBottomSheetGestures(instance);
            return true;
        }

        proto.firstUpdated = function (...args) {
            if (origFirstUpdated) {
                origFirstUpdated.apply(this, args);
            }
            if (ensureAsideDrawerLayout(this) && this.open && this.dialog) {
                this.dialog.setAttribute('open', '');
            }
        };

        proto.show = async function () {
            if (ensureAsideDrawerLayout(this)) {
                this.isOpening = true;
                await this.isConnectedPromise;
                await this.updateComplete;
                ensureAsideDrawerLayout(this);
                if (this.dialog) {
                    if (typeof this.dialog.close === 'function' && this.dialog.matches(':modal')) {
                        this.dialog.close();
                    }
                    this.dialog.setAttribute('open', '');
                }
                if (!isPermanentSheet(this)) {
                    setSheetSnap(this, 'half');
                }
                this.isOpen = true;
                this.isOpening = false;
                ensureSheetScrim(this, this.getAttribute('data-sheet-snap') || 'peek');
                this.dispatchEvent(new Event('opened'));
                return;
            }
            return origShow.apply(this);
        };

        proto.close = async function (returnValue = this.returnValue) {
            if (isBodyAsideDialog(this)) {
                this.isOpening = false;
                await this.updateComplete;
                if (this.dialog) {
                    this.dialog.removeAttribute('open');
                }
                this.isOpen = false;
                this.removeAttribute('open');
                ensureSheetScrim(this, 'peek');
                this.dispatchEvent(new Event('close', { cancelable: true }));
                this.dispatchEvent(new Event('closed'));
                return;
            }
            return origClose.call(this, returnValue);
        };

        // Sync handle visibility & scrim when crossing the 840px breakpoint
        if (typeof window.matchMedia === 'function' && !window.__socraticAsideMqlAttached) {
            window.__socraticAsideMqlAttached = true;
            const mql = window.matchMedia('(max-width: 839.98px)');
            const onViewportChange = () => {
                if (typeof document === 'undefined') return;
                document.querySelectorAll('body > aside > md-dialog').forEach((el) => {
                    syncHandleVisibility(el);
                    if (!isMobileViewport()) {
                        el.removeAttribute('data-sheet-dragging');
                        el.style.removeProperty('--sheet-drag-h');
                    }
                    ensureSheetScrim(el, el.getAttribute('data-sheet-snap') || 'peek');
                });
            };
            if (typeof mql.addEventListener === 'function') {
                mql.addEventListener('change', onViewportChange);
            }
        }

        // Immediately fix any body > aside > md-dialog elements already in the DOM
        if (typeof document !== 'undefined') {
            document.querySelectorAll('body > aside > md-dialog').forEach((el) => {
                ensureAsideDrawerLayout(el);
                if (el.updateComplete && typeof el.updateComplete.then === 'function') {
                    el.updateComplete.then(() => {
                        ensureAsideDrawerLayout(el);
                    });
                }
                if (el.hasAttribute('open') && el.dialog) {
                    if (typeof el.dialog.close === 'function' && el.dialog.matches(':modal')) {
                        el.dialog.close();
                    }
                    el.dialog.setAttribute('open', '');
                }
            });
        }
    }

    const existing = customElements.get('md-dialog');
    if (existing) {
        applyPatch(existing);
    } else {
        customElements.whenDefined('md-dialog').then(() => {
            applyPatch(customElements.get('md-dialog'));
        });
    }
}

patchDialog();
