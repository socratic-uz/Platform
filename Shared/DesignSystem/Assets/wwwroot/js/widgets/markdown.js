let highlightPromise = null;

export function ensureHighlight() {
    if (window.hljs) return Promise.resolve();
    if (highlightPromise) return highlightPromise;

    highlightPromise = new Promise((resolve, reject) => {
        if (!document.querySelector('script[src*="highlight"]')) {
            const script = document.createElement('script');
            script.src = '_content/Markdown/lib/highlight-extensions.js';
            script.onload = () => {
                const checkHljs = () => {
                    if (window.hljs) {
                        resolve();
                    } else {
                        setTimeout(checkHljs, 50);
                    }
                };
                checkHljs();
            };
            script.onerror = () => {
                const cdnScript = document.createElement('script');
                cdnScript.src = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js';
                cdnScript.onload = () => resolve();
                cdnScript.onerror = (err) => reject(err);
                document.body.appendChild(cdnScript);
            };
            document.body.appendChild(script);
        } else {
            const checkHljs = () => {
                if (window.hljs) {
                    resolve();
                } else {
                    setTimeout(checkHljs, 50);
                }
            };
            checkHljs();
        }
    });

    return highlightPromise;
}

export async function highlight() {
    try {
        await ensureHighlight();
        const preTagList = document.querySelectorAll('pre.snippet, pre code');
        for (let i = 0; i < preTagList.length; i++) {
            const pre = preTagList[i];
            const codeTag = pre.tagName.toLowerCase() === 'code' ? pre : pre.querySelector('code');
            if (codeTag && window.hljs && !codeTag._hljsDone) {
                codeTag._hljsDone = true;
                window.hljs.highlightElement(codeTag);
            }
        }
    } catch (e) {
        console.error("Failed to highlight code block:", e);
    }
}

export function addCopyButton() {
    const snippets = document.querySelectorAll('.snippet, pre:has(> code)');
    for (const snippet of snippets) {
        const code = snippet.querySelector('code');
        if (!code || snippet.querySelector('[data-action="copy"]')) continue;

        const button = document.createElement('button');
        button.type = 'button';
        button.dataset.action = 'copy';
        button.textContent = 'Copy';
        button.addEventListener('click', () => {
            navigator.clipboard.writeText(code.innerText);
            button.textContent = 'Copied!';
            setTimeout(() => {
                button.textContent = 'Copy';
            }, 1000);
        });

        snippet.append(button);
    }
}

let mermaidPromise = null;

export function ensureMermaid() {
    if (window.mermaid) return Promise.resolve();
    if (mermaidPromise) return mermaidPromise;

    mermaidPromise = new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = '_content/Markdown/lib/mermaid.js';
        script.onload = () => {
            if (window.mermaid) {
                window.mermaid.initialize({ startOnLoad: false });
                resolve();
            } else {
                reject(new Error("Mermaid was not loaded."));
            }
        };
        script.onerror = () => {
            const cdnScript = document.createElement('script');
            cdnScript.src = 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js';
            cdnScript.onload = () => {
                if (window.mermaid) {
                    window.mermaid.initialize({ startOnLoad: false });
                    resolve();
                } else {
                    reject(new Error("Mermaid CDN failed."));
                }
            };
            cdnScript.onerror = (err) => reject(err);
            document.body.appendChild(cdnScript);
        };
        document.body.appendChild(script);
    });

    return mermaidPromise;
}

export async function runMermaid() {
    try {
        await ensureMermaid();
        if (window.mermaid && typeof window.mermaid.run === 'function') {
            await window.mermaid.run();
        }
    } catch (e) {
        console.error("Failed to run Mermaid:", e);
    }
}

export function autoMountMarkdown() {
    if (typeof document === 'undefined') return;
    if (document.querySelector('pre.snippet, .snippet, pre code')) {
        highlight().catch(() => {});
        addCopyButton();
    }
    if (document.querySelector('pre.mermaid, .mermaid')) {
        runMermaid().catch(() => {});
    }
}
