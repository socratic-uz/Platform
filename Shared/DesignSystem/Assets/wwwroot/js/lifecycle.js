/**
 * Socratic Platform - Universal DOM Lifecycle Dispatcher
 * Component-driven 7-layer lifecycle orchestration.
 * Manages mounting, unmounting, and resource disposal with a single optimized MutationObserver.
 */

const registry = new Map();
let isInitialized = false;
let observer = null;

function handleElement(el, selector, hooks, isAdded) {
    if (!el || el.nodeType !== Node.ELEMENT_NODE) return;
    el._mountedComponents = el._mountedComponents || new Set();

    if (isAdded) {
        if (!el._mountedComponents.has(selector)) {
            el._mountedComponents.add(selector);
            try {
                if (typeof hooks.mount === 'function') {
                    hooks.mount(el);
                }
            } catch (err) {
                console.error(`[Lifecycle] Mount error on "${selector}":`, err, el);
            }
        }
    } else {
        if (el._mountedComponents.has(selector)) {
            el._mountedComponents.delete(selector);
            try {
                if (typeof hooks.unmount === 'function') {
                    hooks.unmount(el);
                }
            } catch (err) {
                console.error(`[Lifecycle] Unmount error on "${selector}":`, err, el);
            }
        }
    }
}

function scanElement(el, isAdded) {
    if (!el || el.nodeType !== Node.ELEMENT_NODE) return;

    for (const [selector, hooks] of registry) {
        if (el.matches && el.matches(selector)) {
            handleElement(el, selector, hooks, isAdded);
        }
        if (el.querySelectorAll) {
            const matches = el.querySelectorAll(selector);
            for (let i = 0; i < matches.length; i++) {
                handleElement(matches[i], selector, hooks, isAdded);
            }
        }
    }
}

export function scanAll() {
    if (typeof document === 'undefined') return;
    for (const [selector, hooks] of registry) {
        const elements = document.querySelectorAll(selector);
        for (let i = 0; i < elements.length; i++) {
            handleElement(elements[i], selector, hooks, true);
        }
    }
}

function initObserver() {
    if (isInitialized || typeof window === 'undefined' || typeof MutationObserver === 'undefined') return;
    if (!document.body) {
        document.addEventListener('DOMContentLoaded', initObserver, { once: true });
        return;
    }

    isInitialized = true;
    scanAll();

    observer = new MutationObserver((mutations) => {
        for (let i = 0; i < mutations.length; i++) {
            const mutation = mutations[i];

            for (let j = 0; j < mutation.addedNodes.length; j++) {
                scanElement(mutation.addedNodes[j], true);
            }

            for (let j = 0; j < mutation.removedNodes.length; j++) {
                scanElement(mutation.removedNodes[j], false);
            }
        }
    });

    observer.observe(document.body, { childList: true, subtree: true });
}

export function registerComponent(selector, hooks = {}) {
    if (!selector) return;
    registry.set(selector, hooks);

    if (typeof document !== 'undefined' && document.body) {
        const elements = document.querySelectorAll(selector);
        for (let i = 0; i < elements.length; i++) {
            handleElement(elements[i], selector, hooks, true);
        }
    }

    if (!isInitialized) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initObserver, { once: true });
        } else {
            initObserver();
        }
    }
}

export function unregisterComponent(selector) {
    registry.delete(selector);
}

if (typeof window !== 'undefined') {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initObserver, { once: true });
    } else {
        initObserver();
    }
}

