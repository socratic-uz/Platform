let highlightPromise = null;

export function ensureHighlight() {
    if (window.hljs) return Promise.resolve();
    if (highlightPromise) return highlightPromise;

    highlightPromise = new Promise((resolve, reject) => {
        if (!document.querySelector('script[src*="highlight-extensions.js"]')) {
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
            script.onerror = (err) => reject(err);
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
        var preTagList = document.querySelectorAll('pre.snippet');
        var numberOfPreTags = preTagList.length;
        for (var i = 0; i < numberOfPreTags; i++) {
            var codeTag = preTagList[i].getElementsByTagName('code');
            if (window.hljs) {
                window.hljs.highlightElement(codeTag[0]);
            }
        }
    } catch (e) {
        console.error("Failed to highlight code block:", e);
    }
}

export function addCopyButton() {
    var snippets = document.querySelectorAll('.snippet');
    var numberOfSnippets = snippets.length;
    for (var i = 0; i < numberOfSnippets; i++) {
        let copyButton = snippets[i].getElementsByClassName("hljs-copy")
        if (copyButton.length === 0) {
            let code = snippets[i].getElementsByTagName('code')[0].innerText;
            snippets[i].innerHTML = snippets[i].innerHTML + '<button class="hljs-copy">Copy</button>'; // append copy button

            copyButton[0].addEventListener("click", function () {
                navigator.clipboard.writeText(code);

                this.innerText = 'Copied!';
                let button = this;
                setTimeout(function () {
                    button.innerText = 'Copy';
                }, 1000)
            });
        }
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
        script.onerror = (err) => reject(err);
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




