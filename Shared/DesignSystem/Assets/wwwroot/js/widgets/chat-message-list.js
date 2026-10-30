// ==========================================================================
// Socratic Chat Widget Web Components:
// 1. <chat-messages>: Smooth auto-scroll behavior on message insertion
// 2. <assistant-message>: Scoped Shadow DOM assistant message renderer
// ==========================================================================

if (typeof window !== 'undefined' && window.customElements) {
    if (!customElements.get('chat-messages')) {
        customElements.define('chat-messages', class ChatMessages extends HTMLElement {
            static _isFirstAutoScroll = true;

            connectedCallback() {
                this._observer = new MutationObserver(mutations => this._scheduleAutoScroll(mutations));
                this._observer.observe(this, { childList: true, attributes: true, subtree: true });
            }

            disconnectedCallback() {
                this._observer?.disconnect();
            }

            _scheduleAutoScroll(mutations) {
                cancelAnimationFrame(this._nextAutoScroll);
                this._nextAutoScroll = requestAnimationFrame(() => {
                    const container = this.closest('.message-list-container') || this.parentElement;
                    const elem = this.lastElementChild;
                    if (!elem) return;

                    const addedUserMessage = mutations.some(m => Array.from(m.addedNodes).some(n => n.parentElement === this && n.classList?.contains('user-message')));
                    if (ChatMessages._isFirstAutoScroll || addedUserMessage || this._elemIsNearScrollBoundary(container, elem, 250)) {
                        elem.scrollIntoView({ behavior: ChatMessages._isFirstAutoScroll ? 'instant' : 'smooth', block: 'end' });
                        ChatMessages._isFirstAutoScroll = false;
                    }
                });
            }

            _elemIsNearScrollBoundary(container, elem, threshold) {
                if (!container) return true;
                const remainingScrollDistance = container.scrollHeight - container.clientHeight - container.scrollTop;
                return remainingScrollDistance < (elem.offsetHeight || 50) + threshold;
            }
        });
    }

    if (!customElements.get('assistant-message')) {
        customElements.define('assistant-message', class AssistantMessage extends HTMLElement {
            static observedAttributes = ['markdown'];

            constructor() {
                super();
                this._shadow = this.attachShadow({ mode: 'open' });
                this._style = document.createElement('style');
                this._style.textContent = `
                    :host { display: block; color: inherit; font-size: inherit; line-height: inherit; }
                    p { margin: 0 0 0.75rem 0; }
                    p:last-child { margin-bottom: 0; }
                    ul, ol { margin: 0 0 0.75rem 1.25rem; padding: 0; }
                    li { margin-bottom: 0.25rem; }
                    pre { background: rgba(0,0,0,0.3); padding: 0.75rem; border-radius: 8px; overflow-x: auto; margin: 0.5rem 0; }
                    code { font-family: monospace; font-size: 0.9em; background: rgba(255,255,255,0.1); padding: 0.1em 0.3em; border-radius: 4px; }
                    pre code { background: transparent; padding: 0; }
                    h1, h2, h3, h4 { margin: 1rem 0 0.5rem 0; font-weight: 600; }
                    hr { border: none; border-top: 1px solid rgba(255,255,255,0.15); margin: 1rem 0; }
                    blockquote { border-left: 3px solid #4152d5; margin: 0.5rem 0; padding-left: 0.75rem; opacity: 0.9; }
                    strong { font-weight: 600; }
                    a { color: #60a5fa; text-decoration: underline; }
                `;
                this._container = document.createElement('div');
                this._shadow.appendChild(this._style);
                this._shadow.appendChild(this._container);
            }

            connectedCallback() {
                this._render();
            }

            attributeChangedCallback(name, oldValue, newValue) {
                if (name === 'markdown') {
                    this._render();
                }
            }

            _render() {
                if (!this._container) return;
                const text = (this.getAttribute('markdown') || '').replace(/<citation.*?<\/citation>/gs, '');
                if (window.marked && typeof window.marked.parse === 'function') {
                    this._container.innerHTML = window.marked.parse(text);
                } else {
                    this._container.textContent = text;
                }
            }
        });
    }
}
