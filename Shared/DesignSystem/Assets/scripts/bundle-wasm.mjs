/**
 * bundle-wasm.mjs — Blazor Webcil .wasm Bundler (Build & Publish)
 *
 * Modes:
 *   --publish-dir <path> : Packs $(PublishDir)wwwroot/_framework/*.wasm (except dotnet.native.*.wasm)
 *                          into _framework/assemblies.<hash8>.bundle (+ .br / .gz),
 *                          patches Web.UI.Client*.lib.module*.js, updates endpoints.json.
 *   --build-dir <path>   : Packs _framework/*.wasm referenced by $(OutDir)Web.UI.staticwebassets.runtime.json
 *                          into $(OutDir)wasm-bundle/_framework/assemblies.<hash8>.bundle (raw, fast for dev),
 *                          writes patched Web.UI.Client.lib.module.js into $(OutDir)wasm-bundle/,
 *                          and updates both runtime.json and endpoints.json.
 */

import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';

function getArg(flag, resolvePath = true) {
  const idx = process.argv.indexOf(flag);
  if (idx === -1 || !process.argv[idx + 1]) return null;
  return resolvePath ? path.resolve(process.argv[idx + 1]) : process.argv[idx + 1];
}

const publishDir = getArg('--publish-dir');
const buildDir = getArg('--build-dir');
const manifestPrefix = getArg('--manifest-prefix', false) || 'Web.UI';
const assetsWwwroot = getArg('--assets-wwwroot');

if (!publishDir && !buildDir) {
  console.error('❌ Missing --publish-dir <path> or --build-dir <path>');
  process.exit(1);
}

/**
 * Packs sorted list of { name, fullPath } Webcil .wasm files into:
 *   [4B count][2B nameLen][utf8 name][4B dataLen][raw bytes]...
 */
function packWasmEntries(entries) {
  const chunks = [];
  const countBuf = Buffer.allocUnsafe(4);
  countBuf.writeUInt32LE(entries.length, 0);
  chunks.push(countBuf);

  let rawTotal = 0;
  for (const { name, fullPath } of entries) {
    const data = fs.readFileSync(fullPath);
    const nameBuf = Buffer.from(name, 'utf8');

    const header = Buffer.allocUnsafe(2 + nameBuf.length + 4);
    header.writeUInt16LE(nameBuf.length, 0);
    nameBuf.copy(header, 2);
    header.writeUInt32LE(data.length, 2 + nameBuf.length);

    chunks.push(header, data);
    rawTotal += data.length;
  }

  const bundleBuffer = Buffer.concat(chunks);
  const sha256Base64 = crypto.createHash('sha256').update(bundleBuffer).digest('base64');
  const hash8 = crypto.createHash('sha256').update(bundleBuffer).digest('hex').slice(0, 8);
  return { bundleBuffer, sha256Base64, hash8, rawTotal, bundleName: `assemblies.${hash8}.wasm` };
}

function cleanOldBundles(dir) {
  if (!fs.existsSync(dir)) return;
  for (const existing of fs.readdirSync(dir)) {
    if (/^assemblies\.[a-f0-9]+\.(bundle|wasm)(\.br|\.gz)?$/.test(existing)) {
      fs.unlinkSync(path.join(dir, existing));
    }
  }
}

function updateEndpointHeaders(ep, buf, sha, nowUtc) {
  const etag = `"${sha}"`;
  for (const h of ep.ResponseHeaders || []) {
    if (h.Name === 'Content-Length') h.Value = String(buf.length);
    if (h.Name === 'ETag') h.Value = etag;
    if (h.Name === 'Last-Modified') h.Value = nowUtc;
  }
  for (const p of ep.EndpointProperties || []) {
    if (p.Name === 'integrity') p.Value = `sha256-${sha}`;
    if (p.Name === 'original-resource') p.Value = etag;
  }
}

function createBundleEndpoint(route, assetFile, encoding, buf, integritySha, nowUtc) {
  const etag = `"${crypto.createHash('sha256').update(buf).digest('base64')}"`;
  const headers = [
    { Name: 'Cache-Control', Value: 'max-age=31536000, immutable' },
    ...(encoding ? [{ Name: 'Content-Encoding', Value: encoding }] : []),
    { Name: 'Content-Length', Value: String(buf.length) },
    { Name: 'Content-Type', Value: 'application/octet-stream' },
    { Name: 'ETag', Value: etag },
    { Name: 'Last-Modified', Value: nowUtc },
    { Name: 'Vary', Value: 'Accept-Encoding' }
  ];
  const selectors = encoding
    ? [{ Name: 'Content-Encoding', Value: encoding, Quality: encoding === 'br' ? '0.0001' : '0.0002' }]
    : [];
  return {
    Route: route,
    AssetFile: assetFile,
    Selectors: selectors,
    ResponseHeaders: headers,
    EndpointProperties: [{ Name: 'integrity', Value: `sha256-${integritySha}` }]
  };
}

if (publishDir) {
  const wwwrootDir = path.join(publishDir, 'wwwroot');
  const frameworkDir = path.join(wwwrootDir, '_framework');
  const endpointsPath = path.join(publishDir, 'Web.UI.staticwebassets.endpoints.json');

  if (!fs.existsSync(frameworkDir)) {
    console.log(`ℹ️ [wasm-bundle] No _framework directory at ${frameworkDir}, skipping.`);
    process.exit(0);
  }

  const wasmFiles = fs
    .readdirSync(frameworkDir)
    .filter((name) => (name.endsWith('.wasm') || (name.startsWith('icudt_') && name.endsWith('.dat'))) && !name.startsWith('dotnet.native.') && !name.startsWith('assemblies.'))
    .sort();

  if (wasmFiles.length === 0) {
    console.log('ℹ️ [wasm-bundle] No Webcil .wasm assemblies found to pack, skipping.');
    process.exit(0);
  }

  const packed = packWasmEntries(
    wasmFiles.map((name) => ({ name, fullPath: path.join(frameworkDir, name) }))
  );
  const bundlePath = path.join(frameworkDir, packed.bundleName);

  cleanOldBundles(frameworkDir);
  fs.writeFileSync(bundlePath, packed.bundleBuffer);
  const brBuffer = zlib.brotliCompressSync(packed.bundleBuffer, {
    params: { [zlib.constants.BROTLI_PARAM_QUALITY]: 11 }
  });
  const gzBuffer = zlib.gzipSync(packed.bundleBuffer, { level: 9 });
  fs.writeFileSync(`${bundlePath}.br`, brBuffer);
  fs.writeFileSync(`${bundlePath}.gz`, gzBuffer);

  for (const fileName of wasmFiles) {
    for (const suffix of ['', '.br', '.gz']) {
      const p = path.join(frameworkDir, fileName + suffix);
      if (fs.existsSync(p)) fs.unlinkSync(p);
    }
  }

  const patchedFiles = [];
  const renamedModules = [];

  function writeWithCompressedVariants(fullPath, buf) {
    fs.writeFileSync(fullPath, buf);
    if (fs.existsSync(`${fullPath}.br`)) {
      fs.writeFileSync(
        `${fullPath}.br`,
        zlib.brotliCompressSync(buf, { params: { [zlib.constants.BROTLI_PARAM_QUALITY]: 11 } })
      );
    }
    if (fs.existsSync(`${fullPath}.gz`)) {
      fs.writeFileSync(`${fullPath}.gz`, zlib.gzipSync(buf, { level: 9 }));
    }
  }

  function patchJsInDir(dir, relPrefix = '') {
    if (!fs.existsSync(dir)) return;
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const full = path.join(dir, entry.name);
      const rel = relPrefix ? `${relPrefix}/${entry.name}` : entry.name;
      if (entry.isDirectory()) {
        patchJsInDir(full, rel);
      } else if (entry.isFile() && /^Web\.UI\.Client.*\.lib\.module.*\.js$/.test(entry.name)) {
        const content = fs.readFileSync(full, 'utf8');
        if (content.includes('assemblies#[.{fingerprint}].wasm') || content.includes('__ASSEMBLIES_BUNDLE_FILE__')) {
          const updatedBuf = Buffer.from(
            content
              .replaceAll('assemblies#[.{fingerprint}].wasm', packed.bundleName)
              .replaceAll('__ASSEMBLIES_BUNDLE_FILE__', packed.bundleName),
            'utf8'
          );
          const fpMatch = entry.name.match(/^Web\.UI\.Client\.([a-z0-9]+)\.lib\.module\.js$/);
          if (fpMatch) {
            const oldFp = fpMatch[1];
            const newFp = crypto.createHash('sha256').update(updatedBuf).digest('hex').slice(0, 10);
            const newName = `Web.UI.Client.${newFp}.lib.module.js`;
            const newFull = path.join(dir, newName);
            const newRel = relPrefix ? `${relPrefix}/${newName}` : newName;

            fs.writeFileSync(newFull, updatedBuf);
            if (fs.existsSync(`${full}.br`)) {
              fs.unlinkSync(`${full}.br`);
              fs.writeFileSync(
                `${newFull}.br`,
                zlib.brotliCompressSync(updatedBuf, { params: { [zlib.constants.BROTLI_PARAM_QUALITY]: 11 } })
              );
            }
            if (fs.existsSync(`${full}.gz`)) {
              fs.unlinkSync(`${full}.gz`);
              fs.writeFileSync(`${newFull}.gz`, zlib.gzipSync(updatedBuf, { level: 9 }));
            }
            if (newFull !== full && fs.existsSync(full)) {
              fs.unlinkSync(full);
            }
            renamedModules.push({ oldRel: rel, newRel, oldName: entry.name, newName, oldFp, newFp });
            patchedFiles.push(newRel);
          } else {
            writeWithCompressedVariants(full, updatedBuf);
            patchedFiles.push(rel);
          }
        }
      }
    }
  }
  patchJsInDir(wwwrootDir);

  if (renamedModules.length > 0) {
    const refTargets = ['Web.UI.modules.json', '_framework/dotnet.js'];
    for (const refRel of refTargets) {
      const refFull = path.join(wwwrootDir, refRel);
      if (!fs.existsSync(refFull)) continue;
      let refText = fs.readFileSync(refFull, 'utf8');
      let changed = false;
      for (const r of renamedModules) {
        if (refText.includes(r.oldName)) {
          refText = refText.replaceAll(r.oldName, r.newName);
          changed = true;
        }
      }
      if (changed) {
        writeWithCompressedVariants(refFull, Buffer.from(refText, 'utf8'));
        patchedFiles.push(refRel);
      }
    }
  }

  if (fs.existsSync(endpointsPath)) {
    const manifest = JSON.parse(fs.readFileSync(endpointsPath, 'utf8'));
    const packedSet = new Set(wasmFiles.map((n) => `_framework/${n}`));
    const nowUtc = new Date().toUTCString();

    manifest.Endpoints = manifest.Endpoints.filter((ep) => {
      const baseAsset = (ep.AssetFile || '').replace(/\.(br|gz)$/, '');
      return !packedSet.has(baseAsset);
    });

    for (const r of renamedModules) {
      for (const ep of manifest.Endpoints) {
        if (ep.Route && ep.Route.includes(r.oldName)) {
          ep.Route = ep.Route.replaceAll(r.oldName, r.newName);
        }
        if (ep.AssetFile && ep.AssetFile.includes(r.oldName)) {
          ep.AssetFile = ep.AssetFile.replaceAll(r.oldName, r.newName);
        }
        for (const p of ep.EndpointProperties || []) {
          if (p.Name === 'fingerprint' && p.Value === r.oldFp) {
            p.Value = r.newFp;
          }
        }
      }
    }

    for (const rel of patchedFiles) {
      const full = path.join(wwwrootDir, rel);
      const variants = [
        { assetFile: rel, buf: fs.readFileSync(full) },
        ...(fs.existsSync(`${full}.br`) ? [{ assetFile: `${rel}.br`, buf: fs.readFileSync(`${full}.br`) }] : []),
        ...(fs.existsSync(`${full}.gz`) ? [{ assetFile: `${rel}.gz`, buf: fs.readFileSync(`${full}.gz`) }] : [])
      ];
      for (const v of variants) {
        const sha = crypto.createHash('sha256').update(v.buf).digest('base64');
        for (const ep of manifest.Endpoints) {
          if (ep.AssetFile === v.assetFile) {
            updateEndpointHeaders(ep, v.buf, sha, nowUtc);
          }
        }
      }
    }

    const route = `_framework/${packed.bundleName}`;
    manifest.Endpoints.push(
      createBundleEndpoint(route, `${route}.br`, 'br', brBuffer, packed.sha256Base64, nowUtc),
      createBundleEndpoint(route, `${route}.gz`, 'gz', gzBuffer, packed.sha256Base64, nowUtc),
      createBundleEndpoint(route, route, null, packed.bundleBuffer, packed.sha256Base64, nowUtc)
    );

    fs.writeFileSync(endpointsPath, JSON.stringify(manifest), 'utf8');
  }

  console.log(
    `✅ [wasm-bundle] Packed ${wasmFiles.length} .wasm assemblies (${(packed.rawTotal / 1024 / 1024).toFixed(2)} MB) -> ${packed.bundleName} (raw ${(packed.bundleBuffer.length / 1024 / 1024).toFixed(2)} MB, br ${(brBuffer.length / 1024 / 1024).toFixed(2)} MB)`
  );
} else if (buildDir) {
  const runtimePath = path.join(buildDir, `${manifestPrefix}.staticwebassets.runtime.json`);
  const endpointsPath = path.join(buildDir, `${manifestPrefix}.staticwebassets.endpoints.json`);

  if (!fs.existsSync(runtimePath) || !fs.existsSync(endpointsPath)) {
    console.log(`ℹ️ [wasm-bundle] StaticWebAssets manifests not found in ${buildDir}, skipping.`);
    process.exit(0);
  }

  // Remove transitive RCL *.staticwebassets.*.json so WasmAppHost doesn't pick Assets.staticwebassets.runtime.json alphabetically before Web.UI.Client
  for (const f of fs.readdirSync(buildDir)) {
    if (
      (f.endsWith('.staticwebassets.runtime.json') || f.endsWith('.staticwebassets.endpoints.json')) &&
      !f.startsWith(`${manifestPrefix}.staticwebassets.`)
    ) {
      fs.rmSync(path.join(buildDir, f), { force: true });
    }
  }

  const runtime = JSON.parse(fs.readFileSync(runtimePath, 'utf8'));
  const endpoints = JSON.parse(fs.readFileSync(endpointsPath, 'utf8'));

  const fwChildren = runtime.Root?.Children?._framework?.Children || {};
  const wasmEntries = [];
  for (const [name, node] of Object.entries(fwChildren)) {
    if ((name.endsWith('.wasm') || (name.startsWith('icudt_') && name.endsWith('.dat'))) && !name.startsWith('dotnet.native.') && !name.startsWith('assemblies.') && node?.Asset) {
      const root = runtime.ContentRoots[node.Asset.ContentRootIndex];
      const fullPath = path.join(root, node.Asset.SubPath);
      if (fs.existsSync(fullPath)) {
        wasmEntries.push({ name, fullPath });
      }
    }
  }
  wasmEntries.sort((a, b) => a.name.localeCompare(b.name));

  const bundleRootDir = path.join(buildDir, 'wasm-bundle') + path.sep;
  const bundleFrameworkDir = path.join(bundleRootDir, '_framework');
  fs.mkdirSync(bundleFrameworkDir, { recursive: true });

  let packed = null;
  if (wasmEntries.length > 0) {
    packed = packWasmEntries(wasmEntries);
    cleanOldBundles(bundleFrameworkDir);
    fs.writeFileSync(path.join(bundleFrameworkDir, packed.bundleName), packed.bundleBuffer);
  } else {
    const existingBundleName = Object.keys(fwChildren).find((k) => /^assemblies\.[a-f0-9]+\.(bundle|wasm)$/.test(k));
    const existingBundlePath = existingBundleName ? path.join(bundleFrameworkDir, existingBundleName) : null;
    if (existingBundleName && existingBundlePath && fs.existsSync(existingBundlePath)) {
      const buf = fs.readFileSync(existingBundlePath);
      packed = {
        bundleName: existingBundleName,
        bundleBuffer: buf,
        rawTotal: buf.length,
        sha256Base64: crypto.createHash('sha256').update(buf).digest('base64')
      };
    } else {
      console.log('ℹ️ [wasm-bundle] No Webcil .wasm assemblies found in runtime manifest, skipping.');
      process.exit(0);
    }
  }

  // Locate and copy+patch Web.UI.Client.lib.module.js without touching source wwwroot/
  const rootChildren = runtime.Root?.Children || {};
  let libSourcePath = null;
  const libKeys = [];
  for (const r of runtime.ContentRoots) {
    if (r !== bundleRootDir && fs.existsSync(path.join(r, 'Web.UI.Client.lib.module.js'))) {
      libSourcePath = path.join(r, 'Web.UI.Client.lib.module.js');
      break;
    }
  }
  for (const [key, node] of Object.entries(rootChildren)) {
    if (/^Web\.UI\.Client.*\.lib\.module\.js$/.test(key) && node?.Asset) {
      libKeys.push(key);
      if (!libSourcePath) {
        const candidate = path.join(runtime.ContentRoots[node.Asset.ContentRootIndex], node.Asset.SubPath);
        if (fs.existsSync(candidate)) libSourcePath = candidate;
      }
    }
  }

  // Also inspect dotnet.js in _framework to ensure any referenced Web.UI.Client.<hash>.lib.module.js is registered
  let dotnetBuf = null;
  let dotnetSha = null;
  const dotnetNode = fwChildren['dotnet.js'];
  if (dotnetNode?.Asset) {
    const dotnetSrc = path.join(runtime.ContentRoots[dotnetNode.Asset.ContentRootIndex], dotnetNode.Asset.SubPath);
    if (fs.existsSync(dotnetSrc)) {
      dotnetBuf = fs.readFileSync(dotnetSrc);
      dotnetSha = crypto.createHash('sha256').update(dotnetBuf).digest('base64');
      const dotnetRaw = dotnetBuf.toString('utf8');
      for (const m of dotnetRaw.matchAll(/Web\.UI\.Client\.[a-z0-9]+\.lib\.module\.js/g)) {
        if (!libKeys.includes(m[0])) libKeys.push(m[0]);
      }
      const gzNode = fwChildren['dotnet.js.gz'];
      if (gzNode?.Asset) {
        const gzPath = path.join(runtime.ContentRoots[gzNode.Asset.ContentRootIndex], gzNode.Asset.SubPath);
        if (fs.existsSync(path.dirname(gzPath))) {
          fs.writeFileSync(gzPath, zlib.gzipSync(dotnetBuf, { level: 6 }));
        }
      }
    }
  }

  let patchedLibBuf = null;
  let patchedLibSha = null;
  if (libSourcePath) {
    const rawContent = fs.readFileSync(libSourcePath, 'utf8');
    const updatedContent = rawContent
      .replaceAll('assemblies#[.{fingerprint}].wasm', packed.bundleName)
      .replaceAll('__ASSEMBLIES_BUNDLE_FILE__', packed.bundleName);
    patchedLibBuf = Buffer.from(updatedContent, 'utf8');
    patchedLibSha = crypto.createHash('sha256').update(patchedLibBuf).digest('base64');
    fs.writeFileSync(path.join(bundleRootDir, 'Web.UI.Client.lib.module.js'), patchedLibBuf);
  }

  // Build map of clean asset path -> fingerprinted route from endpoints.json using the 'fingerprint' EndpointProperty
  const fingerprintedByAsset = new Map();
  for (const ep of endpoints.Endpoints || []) {
    const route = ep.Route || '';
    if (!route || route.endsWith('.gz') || route.endsWith('.br')) {
      continue;
    }
    const fpProp = (ep.EndpointProperties || []).find((p) => p.Name === 'fingerprint' && p.Value);
    if (fpProp) {
      const cleanAsset = route.replace(`.${fpProp.Value}.`, '.');
      if (cleanAsset !== route) {
        fingerprintedByAsset.set(cleanAsset, route);
      }
    }
  }

  function buildFingerprintedImportMap() {
    const imports = {};
    for (const [asset, route] of fingerprintedByAsset.entries()) {
      if (asset.endsWith('.js') || asset.endsWith('.mjs')) {
        imports[`./${asset}`] = `./${route}`;
      }
    }
    return JSON.stringify({ imports });
  }

  function resolveHtmlFingerprints(html, useFingerprintedRoutes) {
    let out = html
      .replaceAll('assemblies#[.{fingerprint}].wasm', packed.bundleName)
      .replaceAll('__ASSEMBLIES_BUNDLE_FILE__', packed.bundleName);
    out = out.replace(/(\.?\/?[a-zA-Z0-9_./-]+)#\[\.\{fingerprint\}\](\.[a-zA-Z0-9]+)/g, (_, prefix, ext) => {
      const hasDotSlash = prefix.startsWith('./');
      const hasLeadSlash = prefix.startsWith('/');
      const cleanRel = (hasDotSlash ? prefix.slice(2) : hasLeadSlash ? prefix.slice(1) : prefix) + ext;
      if (useFingerprintedRoutes && fingerprintedByAsset.has(cleanRel)) {
        const fpRoute = fingerprintedByAsset.get(cleanRel);
        return (hasDotSlash ? './' : hasLeadSlash ? '/' : '') + fpRoute;
      }
      return prefix + ext;
    });
    if (out.includes('<script type="importmap"></script>') && useFingerprintedRoutes) {
      out = out.replace('<script type="importmap"></script>', `<script type="importmap">${buildFingerprintedImportMap()}</script>`);
    }
    return out;
  }

  let rawIndexHtml = null;
  let patchedIndexBuf = null;
  let patchedIndexSha = null;
  const indexNode = rootChildren['index.html'];
  if (indexNode?.Asset) {
    let indexSrcPath = null;
    for (const r of runtime.ContentRoots) {
      if (r !== bundleRootDir && !r.includes('Assets') && fs.existsSync(path.join(r, 'index.html'))) {
        indexSrcPath = path.join(r, 'index.html');
        break;
      }
    }
    if (!indexSrcPath) {
      indexSrcPath = path.join(runtime.ContentRoots[indexNode.Asset.ContentRootIndex], indexNode.Asset.SubPath);
    }
    if (fs.existsSync(indexSrcPath)) {
      const rawHtml = fs.readFileSync(indexSrcPath, 'utf8');
      if (rawHtml.includes('#[.{fingerprint}]') || rawHtml.includes('__ASSEMBLIES_BUNDLE_FILE__')) {
        rawIndexHtml = rawHtml;
        patchedIndexBuf = Buffer.from(resolveHtmlFingerprints(rawHtml, true), 'utf8');
        patchedIndexSha = crypto.createHash('sha256').update(patchedIndexBuf).digest('base64');
        fs.writeFileSync(path.join(bundleRootDir, 'index.html'), patchedIndexBuf);
      }
    }
  }

  // Update runtime.json
  let bundleRootIdx = runtime.ContentRoots.indexOf(bundleRootDir);
  if (bundleRootIdx === -1) {
    bundleRootIdx = runtime.ContentRoots.length;
    runtime.ContentRoots.push(bundleRootDir);
  }

  const packedNames = new Set(wasmEntries.map((e) => e.name));
  for (const key of Object.keys(fwChildren)) {
    const baseKey = key.replace(/\.(br|gz)$/, '');
    if (packedNames.has(baseKey) || /^assemblies\.[a-f0-9]+\.(bundle|wasm)(\.br|\.gz)?$/.test(key)) {
      delete fwChildren[key];
    }
  }
  fwChildren[packed.bundleName] = {
    Children: null,
    Asset: { ContentRootIndex: bundleRootIdx, SubPath: `_framework/${packed.bundleName}` },
    Patterns: null
  };

  if (patchedLibBuf) {
    for (const key of Object.keys(rootChildren)) {
      if (/^Web\.UI\.Client.*\.lib\.module\.js\.(br|gz)$/.test(key)) {
        delete rootChildren[key];
      }
    }
    for (const key of libKeys) {
      rootChildren[key] = {
        Children: null,
        Asset: { ContentRootIndex: bundleRootIdx, SubPath: 'Web.UI.Client.lib.module.js' },
        Patterns: null
      };
    }
  }

  if (patchedIndexBuf && rootChildren['index.html']?.Asset) {
    delete rootChildren['index.html.br'];
    delete rootChildren['index.html.gz'];
    rootChildren['index.html'].Asset = { ContentRootIndex: bundleRootIdx, SubPath: 'index.html' };
  }

  delete fwChildren['dotnet.js.br'];
  delete fwChildren['dotnet.js.gz'];

  fs.writeFileSync(runtimePath, JSON.stringify(runtime), 'utf8');

  // Update endpoints.json
  const nowUtc = new Date().toUTCString();
  const packedAssets = new Set(wasmEntries.map((e) => `_framework/${e.name}`));
  endpoints.Endpoints = endpoints.Endpoints.filter((ep) => {
    const asset = ep.AssetFile || '';
    const baseAsset = asset.replace(/\.(br|gz)$/, '');
    if (packedAssets.has(baseAsset)) return false;
    if (/^_framework\/assemblies\.[a-f0-9]+\.(bundle|wasm)(\.br|\.gz)?$/.test(asset)) return false;
    if (/^Web\.UI\.Client.*\.lib\.module\.js\.(br|gz)$/.test(asset)) return false;
    if (/^_framework\/dotnet(\.[a-z0-9]+)?\.js\.(br|gz)$/.test(asset)) return false;
    if (patchedIndexBuf && /^index\.html\.(br|gz)$/.test(asset)) return false;
    return true;
  });

  if (dotnetBuf && dotnetSha) {
    for (const ep of endpoints.Endpoints) {
      if ((ep.AssetFile || '') === '_framework/dotnet.js') {
        updateEndpointHeaders(ep, dotnetBuf, dotnetSha, nowUtc);
      }
    }
  }

  if (patchedLibBuf && patchedLibSha) {
    let libEndpointTemplate = null;
    for (const ep of endpoints.Endpoints) {
      if (/^Web\.UI\.Client.*\.lib\.module\.js$/.test(ep.AssetFile || '')) {
        updateEndpointHeaders(ep, patchedLibBuf, patchedLibSha, nowUtc);
        if (!libEndpointTemplate) libEndpointTemplate = ep;
      }
    }
    for (const key of libKeys) {
      if (!endpoints.Endpoints.some((ep) => ep.Route === key)) {
        const cloned = libEndpointTemplate
          ? JSON.parse(JSON.stringify(libEndpointTemplate))
          : {
              Route: key,
              AssetFile: key,
              Selectors: [],
              ResponseHeaders: [
                { Name: 'Cache-Control', Value: 'no-cache' },
                { Name: 'Content-Length', Value: String(patchedLibBuf.length) },
                { Name: 'Content-Type', Value: 'text/javascript' },
                { Name: 'ETag', Value: `"${patchedLibSha}"` },
                { Name: 'Last-Modified', Value: nowUtc }
              ],
              EndpointProperties: [{ Name: 'integrity', Value: `sha256-${patchedLibSha}` }]
            };
        cloned.Route = key;
        cloned.AssetFile = key;
        updateEndpointHeaders(cloned, patchedLibBuf, patchedLibSha, nowUtc);
        endpoints.Endpoints.push(cloned);
      }
    }
  }

  if (patchedIndexBuf && patchedIndexSha) {
    let indexEndpointTemplate = null;
    for (const ep of endpoints.Endpoints) {
      if ((ep.AssetFile || '') === 'index.html') {
        updateEndpointHeaders(ep, patchedIndexBuf, patchedIndexSha, nowUtc);
        if (ep.Route === 'index.html') indexEndpointTemplate = ep;
      }
    }
    if (indexEndpointTemplate && !endpoints.Endpoints.some((ep) => ep.Route === '')) {
      endpoints.Endpoints.push({
        ...indexEndpointTemplate,
        Route: ''
      });
    }
  }

  const route = `_framework/${packed.bundleName}`;
  endpoints.Endpoints.push(
    createBundleEndpoint(route, route, null, packed.bundleBuffer, packed.sha256Base64, nowUtc)
  );

  // Ensure _framework/blazor.webassembly.js is registered if index.html exists in this build (WasmAppHost standalone)
  if (indexNode && !fwChildren['blazor.webassembly.js']) {
    const nugetRoot = process.env.NUGET_PACKAGES || 'D:\\packages\\nuget';
    const wasmPkgDir = path.join(nugetRoot, 'microsoft.aspnetcore.components.webassembly');
    if (fs.existsSync(wasmPkgDir)) {
      for (const ver of fs.readdirSync(wasmPkgDir).sort().reverse()) {
        const candidate = path.join(wasmPkgDir, ver, 'build', 'net10.0', 'blazor.webassembly.js');
        if (fs.existsSync(candidate)) {
          const jsBuf = fs.readFileSync(candidate);
          const jsSha = crypto.createHash('sha256').update(jsBuf).digest('base64');
          fs.writeFileSync(path.join(bundleFrameworkDir, 'blazor.webassembly.js'), jsBuf);
          fwChildren['blazor.webassembly.js'] = {
            Children: null,
            Asset: { ContentRootIndex: bundleRootIdx, SubPath: '_framework/blazor.webassembly.js' },
            Patterns: null
          };
          fs.writeFileSync(runtimePath, JSON.stringify(runtime), 'utf8');
          const ep = createBundleEndpoint('_framework/blazor.webassembly.js', '_framework/blazor.webassembly.js', null, jsBuf, jsSha, nowUtc);
          ep.ResponseHeaders.find((h) => h.Name === 'Content-Type').Value = 'text/javascript';
          endpoints.Endpoints.push(ep);
          break;
        }
      }
    }
  }

  fs.writeFileSync(endpointsPath, JSON.stringify(endpoints), 'utf8');

  // Optionally sync modified index.html + _framework/ + _content/ + appsettings*.json into Assets/wwwroot/ for standalone file:// mode & analytics
  if (assetsWwwroot && patchedIndexBuf) {
    const assetsFwDir = path.join(assetsWwwroot, '_framework');
    fs.rmSync(assetsFwDir, { recursive: true, force: true });
    fs.mkdirSync(assetsFwDir, { recursive: true });

    // Copy bundle + blazor.webassembly.js from wasm-bundle/_framework
    for (const f of fs.readdirSync(bundleFrameworkDir)) {
      fs.copyFileSync(path.join(bundleFrameworkDir, f), path.join(assetsFwDir, f));
    }

    // Copy remaining runtime JS/native WASM (dotnet.js, dotnet.runtime.*.js, dotnet.native.*) from runtime manifest
    for (const [name, node] of Object.entries(fwChildren)) {
      if (
        node?.Asset &&
        !name.endsWith('.br') &&
        !name.endsWith('.gz') &&
        !name.endsWith('.pdb') &&
        !name.startsWith('assemblies.') &&
        name !== 'blazor.webassembly.js' &&
        !name.includes('HotReload')
      ) {
        const src = path.join(runtime.ContentRoots[node.Asset.ContentRootIndex], node.Asset.SubPath);
        if (fs.existsSync(src)) {
          if (name === 'dotnet.js') {
            const raw = fs.readFileSync(src, 'utf8');
            const m = raw.match(/(withConfig\(\/\*json-start\*\/)([\s\S]*?)(\/\*json-end\*\/)/);
            if (m) {
              const cfg = JSON.parse(m[2]);
              if (cfg?.resources?.libraryInitializers) delete cfg.resources.libraryInitializers;
              if (cfg?.resources?.jsModuleInitializer) delete cfg.resources.jsModuleInitializer;
              if (cfg?.resources?.pdb) delete cfg.resources.pdb;
              cfg.debugLevel = 0;
              fs.writeFileSync(
                path.join(assetsFwDir, name),
                raw.slice(0, m.index) + m[1] + JSON.stringify(cfg, null, 2) + m[3] + raw.slice(m.index + m[0].length),
                'utf8'
              );
            } else {
              fs.copyFileSync(src, path.join(assetsFwDir, name));
            }
          } else {
            fs.copyFileSync(src, path.join(assetsFwDir, name));
          }
        }
      }
    }

    // Copy non-Assets RCL _content/* (e.g., QuickGrid/QuickGrid.razor.js, Markdown, Blazor3D) into Assets/wwwroot/_content/
    const assetsContentDir = path.join(assetsWwwroot, '_content');
    fs.rmSync(assetsContentDir, { recursive: true, force: true });
    const contentChildren = rootChildren['_content']?.Children || {};
    function copyContentTree(children, destDir) {
      for (const [name, node] of Object.entries(children)) {
        if (name.endsWith('.br') || name.endsWith('.gz')) continue;
        if (node?.Asset) {
          const src = path.join(runtime.ContentRoots[node.Asset.ContentRootIndex], node.Asset.SubPath);
          if (fs.existsSync(src)) {
            fs.mkdirSync(destDir, { recursive: true });
            fs.copyFileSync(src, path.join(destDir, name));
          }
        }
        if (node?.Children) {
          copyContentTree(node.Children, path.join(destDir, name));
        }
      }
    }
    for (const [pkg, pkgNode] of Object.entries(contentChildren)) {
      if (pkg === 'Assets' || !pkgNode?.Children) continue;
      copyContentTree(pkgNode.Children, path.join(assetsContentDir, pkg));
    }

    // Copy appsettings*.json into Assets/wwwroot/
    for (const [name, node] of Object.entries(rootChildren)) {
      if (/^appsettings(\.[a-zA-Z]+)?\.json$/i.test(name) && node?.Asset) {
        const src = path.join(runtime.ContentRoots[node.Asset.ContentRootIndex], node.Asset.SubPath);
        if (fs.existsSync(src)) {
          fs.copyFileSync(src, path.join(assetsWwwroot, name));
        }
      }
    }

    // Write modified index.html with ./_content/Assets/ -> ./, importmap for dynamic ES module imports, and file:// file-shim.js for Assets/wwwroot/
    let assetsHtml = resolveHtmlFingerprints(rawIndexHtml || patchedIndexBuf.toString('utf8'), false)
      .replaceAll('./_content/Assets/', './')
      .replace(
        '<script type="importmap"></script>',
        '<script type="importmap">{"imports":{"./_content/Assets/":"./","/_content/Assets/":"./"}}</script>'
      );
    if (!assetsHtml.includes('file-shim.js')) {
      assetsHtml = assetsHtml.replace(
        '<script src="./js/layouts/theme-init.js"></script>',
        '<script src="./js/layouts/theme-init.js"></script>\n  <script src="./js/file-shim.js"></script>'
      );
    }
    fs.writeFileSync(path.join(assetsWwwroot, 'index.html'), assetsHtml, 'utf8');
  }

  console.log(
    `✅ [wasm-bundle] Packed ${wasmEntries.length} .wasm assemblies (${(packed.rawTotal / 1024 / 1024).toFixed(2)} MB) -> ${packed.bundleName} (dev build)`
  );
}
