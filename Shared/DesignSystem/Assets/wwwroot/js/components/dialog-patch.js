/* ==========================================================================
   Material Web Components - Aside Drawer Dialog Patch
   Transforms <aside> > <md-dialog> into a non-modal docked Drawer
   (Side Sheet on Desktop / Bottom Sheet on Mobile) without Top-Layer lock.
   ========================================================================== */

export function patchDialog() {
    if (typeof window === 'undefined' || typeof customElements === 'undefined') {
        return;
    }

    customElements.whenDefined('md-dialog').then(() => {
        const dialogClass = customElements.get('md-dialog');
        if (!dialogClass || dialogClass.prototype.__asidePatched) {
            return;
        }

        const proto = dialogClass.prototype;
        proto.__asidePatched = true;

        const origShow = proto.show;
        const origClose = proto.close;
        const origFirstUpdated = proto.firstUpdated;

        function ensureAsideDrawerLayout(instance) {
            if (!instance.closest || !instance.closest('aside')) {
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
            if (shadow && !shadow.__asideStyled) {
                shadow.__asideStyled = true;
                const style = document.createElement('style');
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
                    .scroller {
                        flex: 1 1 auto !important;
                        overflow-y: auto !important;
                        display: flex !important;
                        flex-direction: column !important;
                    }
                    .content {
                        flex: 1 1 auto !important;
                        height: auto !important;
                    }
                    slot[name=content]::slotted(*) {
                        padding: 16px !important;
                    }
                    slot[name=headline]::slotted(*) {
                        padding: 12px 16px !important;
                    }
                `;
                shadow.appendChild(style);
            }

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
                    this.dialog.setAttribute('open', '');
                }
                this.isOpen = true;
                this.isOpening = false;
                this.dispatchEvent(new Event('opened'));
                return;
            }
            return origShow.apply(this);
        };

        proto.close = async function (returnValue = this.returnValue) {
            if (this.closest && this.closest('aside')) {
                this.isOpening = false;
                await this.updateComplete;
                if (this.dialog) {
                    this.dialog.removeAttribute('open');
                }
                this.isOpen = false;
                this.removeAttribute('open');
                this.dispatchEvent(new Event('close', { cancelable: true }));
                this.dispatchEvent(new Event('closed'));
                return;
            }
            return origClose.call(this, returnValue);
        };
    });
}

patchDialog();
