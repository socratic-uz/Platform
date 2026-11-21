/**
 * wasm-boot.js — Boots standalone Blazor WASM and serves assemblies + ICU from single bundle
 */
(function () {
  const bundleMeta = document.querySelector('meta[name="wasm-bundle"]');
  const bundleFile = (bundleMeta && bundleMeta.getAttribute('content')) || '';
  let bundlePromise = null;

  function parseBundle(buffer) {
    const view = new DataView(buffer);
    const bytes = new Uint8Array(buffer);
    const decoder = new TextDecoder();
    const entries = new Map();
    if (buffer.byteLength < 4) return entries;
    const count = view.getUint32(0, true);
    let offset = 4;
    for (let i = 0; i < count; i++) {
      const nameLen = view.getUint16(offset, true);
      offset += 2;
      const name = decoder.decode(bytes.subarray(offset, offset + nameLen));
      offset += nameLen;
      const dataLen = view.getUint32(offset, true);
      offset += 4;
      entries.set(name, bytes.subarray(offset, offset + dataLen));
      offset += dataLen;
    }
    return entries;
  }

  function getBundleMap() {
    if (!bundlePromise) {
      bundlePromise = fetch('./_framework/' + bundleFile)
        .then((r) => {
          if (!r.ok) throw new Error('HTTP ' + r.status);
          return r.arrayBuffer();
        })
        .then(parseBundle);
    }
    return bundlePromise;
  }

  function startBlazor() {
    if (typeof Blazor === 'undefined') return;

    if (location.protocol === 'file:' && Blazor._internal && Blazor._internal.navigationManager) {
      const nm = Blazor._internal.navigationManager;
      const origNavigateTo = nm.navigateTo.bind(nm);
      let lastProgrammaticPath = '';
      nm.navigateTo = function (uri, options) {
        if (typeof uri === 'string' && !/^https?:/i.test(uri)) {
          const hashIdx = uri.indexOf('#');
          let target = hashIdx >= 0 ? uri.slice(hashIdx + 1) : uri;
          if (!target.startsWith('/')) {
            const cleanBase = location.href.split('?')[0].split('#')[0];
            const dir = cleanBase.slice(0, cleanBase.lastIndexOf('/') + 1);
            if (target.startsWith(dir)) target = '/' + target.slice(dir.length);
            else if (/^[a-zA-Z0-9_-]+$/.test(target)) target = '/' + target;
          }
          const basePath = target.split('?')[0] || '/';
          if (basePath === '/index.html') return;
          const currentPath = (location.hash.slice(1) || '/').split('?')[0] || '/';
          if (basePath !== currentPath && basePath !== lastProgrammaticPath && window.__fileNavigate) {
            lastProgrammaticPath = basePath;
            window.__fileNavigate(basePath);
          }
          return;
        }
        return origNavigateTo(uri, options);
      };
    }

    const configureRuntime = function (dotnet) {
      if (dotnet && dotnet.withEnvironmentVariable) {
        dotnet.withEnvironmentVariable('STANDALONE_WASM', '1');
      }
    };

    if (bundleFile && !bundleFile.includes('#[') && !bundleFile.startsWith('__')) {
      getBundleMap().catch(() => { });
      Blazor.start({
        configureRuntime,
        loadBootResource: function (type, name, defaultUri, integrity) {
          if ((type === 'assembly' || type === 'globalization') && !name.startsWith('dotnet.native')) {
            const uriFile = (defaultUri || '').split('/').pop().split('?')[0];
            return getBundleMap().then((map) => {
              const data = map.get(uriFile) || map.get(name);
              if (data) {
                return new Response(data, {
                  status: 200,
                  headers: {
                    'Content-Type': uriFile.endsWith('.wasm') ? 'application/wasm' : 'application/octet-stream'
                  }
                });
              }
              return fetch(defaultUri, { integrity });
            });
          }
          return undefined;
        }
      });
    } else {
      Blazor.start({ configureRuntime });
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', startBlazor, { once: true });
  } else {
    startBlazor();
  }
})();
