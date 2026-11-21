/**
 * Blazor WebAssembly JS Initializer — Single-Bundle Assembly Loader
 *
 * In builds where "__ASSEMBLIES_BUNDLE_FILE__" is patched to "assemblies.<hash>.wasm",
 * all .wasm assemblies (except dotnet.native.*.wasm) are fetched in a single HTTP request
 * and served to dotnet.js from memory via loadBootResource.
 */

const BUNDLE_FILE = "assemblies#[.{fingerprint}].wasm";
let bundlePromise = null;

function isBundlingEnabled() {
  return Boolean(BUNDLE_FILE && !BUNDLE_FILE.includes("#[") && !BUNDLE_FILE.startsWith("__"));
}

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
    bundlePromise = fetch(`./_framework/${BUNDLE_FILE}`, { credentials: "same-origin" })
      .then((resp) => {
        if (!resp.ok) {
          throw new Error(`Failed to fetch ${BUNDLE_FILE}: HTTP ${resp.status}`);
        }
        return resp.arrayBuffer();
      })
      .then(parseBundle);
  }
  return bundlePromise;
}

function attachBootResourceLoader(wasmOptions) {
  if (!wasmOptions || wasmOptions.__bundleAttached || !isBundlingEnabled()) {
    return;
  }
  wasmOptions.__bundleAttached = true;

  // Eagerly start fetching the single assembly bundle before dotnet.js requests assemblies
  getBundleMap().catch(() => {});

  const prevLoader = wasmOptions.loadBootResource;
  wasmOptions.loadBootResource = function (type, name, defaultUri, integrity, behavior) {
    if ((type === "assembly" || type === "globalization") && !name.startsWith("dotnet.native")) {
      const uriFile = (defaultUri || "").split("/").pop().split("?")[0];
      return getBundleMap()
        .then((map) => {
          const data = map.get(uriFile) || map.get(name);
          if (data) {
            return new Response(data, {
              status: 200,
              headers: {
                "Content-Type": uriFile.endsWith(".wasm") ? "application/wasm" : "application/octet-stream",
              },
            });
          }
          return prevLoader
            ? prevLoader(type, name, defaultUri, integrity, behavior)
            : fetch(defaultUri, { integrity });
        })
        .catch(() =>
          prevLoader
            ? prevLoader(type, name, defaultUri, integrity, behavior)
            : fetch(defaultUri, { integrity })
        );
    }

    return prevLoader ? prevLoader(type, name, defaultUri, integrity, behavior) : undefined;
  };
}

// Standalone Blazor WebAssembly (blazor.webassembly.js)
export function beforeStart(options) {
  attachBootResourceLoader(options);
}

// Blazor Web App (blazor.web.js): runs BEFORE n.withResourceLoader(e.loadBootResource)
export function beforeWebStart(options) {
  if (!isBundlingEnabled() || !options) {
    return;
  }
  options.webAssembly = options.webAssembly || {};
  attachBootResourceLoader(options.webAssembly);
}

// Fallback
export function beforeWebAssemblyStart(options) {
  attachBootResourceLoader(options);
}
