/**
 * file-shim.js — Runtime shim for standalone Blazor WASM over file://
 */
(function () {
  if (location.protocol !== 'file:') return;

  let fileRouterRef = null;
  window.__registerFileRouter = function (ref) {
    fileRouterRef = ref;
  };
  window.__fileNavigate = function (routePath) {
    if (fileRouterRef && fileRouterRef.invokeMethodAsync) {
      fileRouterRef.invokeMethodAsync('NavigateFileRouteAsync', routePath).catch(() => {});
    }
  };

  const origFetch = window.fetch.bind(window);

  // Normalize /_content/Assets/... -> ./... for file://
  function normalizeAssetUrl(val) {
    if (typeof val !== 'string') return val;
    if (val.startsWith('/_content/Assets/')) return './' + val.slice('/_content/Assets/'.length);
    if (val.startsWith('_content/Assets/')) return './' + val.slice('_content/Assets/'.length);
    if (val.startsWith('/_content/')) return './' + val.slice(1);
    return val;
  }

  const origSetAttribute = Element.prototype.setAttribute;
  Element.prototype.setAttribute = function (name, value) {
    if (name === 'src' || name === 'href' || name === 'poster') {
      value = normalizeAssetUrl(value);
    }
    return origSetAttribute.call(this, name, value);
  };

  function toHashRoute(rawUrl) {
    if (!rawUrl) return '#/';
    const hashPos = rawUrl.indexOf('#');
    if (hashPos !== -1) return rawUrl.slice(hashPos);
    if (rawUrl.startsWith('/')) return '#' + rawUrl;
    const cleanHref = location.href.split('?')[0].split('#')[0];
    const base = cleanHref.slice(0, cleanHref.lastIndexOf('/') + 1);
    if (rawUrl.startsWith(base)) {
      let rel = rawUrl.slice(base.length);
      if (rel.toLowerCase().startsWith('index.html')) rel = rel.slice('index.html'.length);
      while (rel.startsWith('/')) rel = rel.slice(1);
      return '#/' + rel;
    }
    if (rawUrl.startsWith('./')) rawUrl = rawUrl.slice(2);
    return '#/' + rawUrl;
  }

  let isUpdatingHash = false;
  let lastHashSetAt = 0;
  function setHashFromHistory(fallbackFn, state, title, url) {
    const targetHash = toHashRoute(url);
    if (targetHash === '#/' && location.hash && location.hash !== '#/' && location.hash !== '#') {
      return;
    }
    if (targetHash && location.hash !== targetHash) {
      const now = Date.now();
      if (now - lastHashSetAt < 100) return;
      lastHashSetAt = now;
      isUpdatingHash = true;
      location.hash = targetHash;
      isUpdatingHash = false;
      return;
    }
    try { return fallbackFn(state, title, targetHash); } catch {}
  }

  const origPushState = history.pushState.bind(history);
  const origReplaceState = history.replaceState.bind(history);
  history.pushState = function (state, title, url) {
    if (typeof url === 'string' && !url.startsWith('#')) {
      return setHashFromHistory(origPushState, state, title, url);
    }
    try { return origPushState(state, title, url); } catch {
      if (typeof url === 'string') return setHashFromHistory(origPushState, state, title, url);
    }
  };
  let lastReplaceCount = 0;
  let lastReplaceWindowStart = 0;
  history.replaceState = function (state, title, url) {
    if (typeof url === 'string' && !url.startsWith('#')) {
      const now = Date.now();
      if (now - lastReplaceWindowStart > 1000) {
        lastReplaceWindowStart = now;
        lastReplaceCount = 0;
      }
      if (++lastReplaceCount > 4) return;
      try { if (new URL(url, location.href).href === location.href) return; } catch {}
    }
    try { return origReplaceState(state, title, url); } catch {
      if (typeof url === 'string') return setHashFromHistory(origReplaceState, state, title, url);
    }
  };

  function rewriteAssetAttr(el, attr) {
    const val = el.getAttribute && el.getAttribute(attr);
    if (!val) return;
    const next = normalizeAssetUrl(val);
    if (next !== val) origSetAttribute.call(el, attr, next);
  }

  const assetObserver = new MutationObserver(function (mutations) {
    for (const m of mutations) {
      if (m.type === 'attributes') {
        rewriteAssetAttr(m.target, m.attributeName);
      } else if (m.addedNodes) {
        for (const node of m.addedNodes) {
          if (node.nodeType !== 1) continue;
          rewriteAssetAttr(node, 'src');
          rewriteAssetAttr(node, 'href');
          rewriteAssetAttr(node, 'poster');
          if (node.querySelectorAll) {
            for (const child of node.querySelectorAll('[src],[href],[poster]')) {
              rewriteAssetAttr(child, 'src');
              rewriteAssetAttr(child, 'href');
              rewriteAssetAttr(child, 'poster');
            }
          }
        }
      }
    }
  });
  assetObserver.observe(document.documentElement, {
    childList: true,
    subtree: true,
    attributes: true,
    attributeFilter: ['src', 'href', 'poster']
  });

  document.addEventListener('click', function (e) {
    if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
    const path = e.composedPath ? e.composedPath() : [e.target];
    let href = null;
    for (const node of path) {
      if (!node || !node.getAttribute) continue;
      if (node.hasAttribute && node.hasAttribute('download')) return;
      const target = node.getAttribute('target');
      if (target && target !== '_self') return;
      const h = node.getAttribute('href');
      if (h) {
        href = h;
        break;
      }
    }
    if (href && href.startsWith('/') && !href.startsWith('//') && !href.startsWith('/_content/') && !href.startsWith('/_framework/')) {
      e.preventDefault();
      e.stopImmediatePropagation();
      location.hash = '#' + href;
    }
  }, true);

  window.addEventListener('hashchange', function () {
    if (isUpdatingHash) return;
    try {
      window.dispatchEvent(new PopStateEvent('popstate', { state: history.state }));
    } catch {}
  });

  const mimeByExt = {
    '.wasm': 'application/wasm',
    '.js': 'application/javascript',
    '.mjs': 'application/javascript',
    '.json': 'application/json',
    '.css': 'text/css',
    '.dat': 'application/octet-stream',
    '.svg': 'image/svg+xml',
    '.png': 'image/png',
    '.woff2': 'font/woff2'
  };

  function getMime(url) {
    const clean = url.split('?')[0].split('#')[0].toLowerCase();
    const dot = clean.lastIndexOf('.');
    return dot !== -1 ? (mimeByExt[clean.slice(dot)] || 'application/octet-stream') : 'application/octet-stream';
  }

  window.fetch = function (input, init) {
    let rawUrl = typeof input === 'string' ? input : (input && input.url ? input.url : '');
    const assetsIdx = rawUrl.indexOf('_content/Assets/');
    if (assetsIdx !== -1) {
      rawUrl = './' + rawUrl.slice(assetsIdx + '_content/Assets/'.length);
    }
    const resolved = rawUrl ? new URL(rawUrl, location.href).href : '';
    if (!resolved.startsWith('file://')) {
      return origFetch(input, init);
    }
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhr.open((init && init.method) || 'GET', resolved, true);
      xhr.responseType = 'arraybuffer';
      xhr.onload = function () {
        if (xhr.status === 200 || xhr.status === 0) {
          resolve(new Response(xhr.response, {
            status: 200,
            statusText: 'OK',
            headers: { 'Content-Type': getMime(resolved) }
          }));
        } else {
          resolve(new Response(null, { status: xhr.status, statusText: xhr.statusText }));
        }
      };
      xhr.onerror = function () {
        if (/\/appsettings(\.[^/]+)?\.json$/i.test(resolved.split('?')[0])) {
          resolve(new Response('{}', {
            status: 200,
            statusText: 'OK',
            headers: { 'Content-Type': 'application/json' }
          }));
          return;
        }
        reject(new TypeError('Failed to fetch local file: ' + resolved));
      };
      xhr.send();
    });
  };

  const origInstantiateStreaming = WebAssembly.instantiateStreaming;
  WebAssembly.instantiateStreaming = async function (source, importObject) {
    try {
      const resp = await source;
      if (!resp.url || resp.url.startsWith('file://')) {
        const buf = await resp.arrayBuffer();
        return WebAssembly.instantiate(buf, importObject);
      }
      return origInstantiateStreaming(Promise.resolve(resp), importObject);
    } catch {
      const resp = await source;
      const buf = await resp.arrayBuffer();
      return WebAssembly.instantiate(buf, importObject);
    }
  };

  const origCompileStreaming = WebAssembly.compileStreaming;
  WebAssembly.compileStreaming = async function (source) {
    try {
      const resp = await source;
      if (!resp.url || resp.url.startsWith('file://')) {
        const buf = await resp.arrayBuffer();
        return WebAssembly.compile(buf);
      }
      return origCompileStreaming(Promise.resolve(resp));
    } catch {
      const resp = await source;
      const buf = await resp.arrayBuffer();
      return WebAssembly.compile(buf);
    }
  };
})();
