/**
 * generate-codegen.mjs — Parallel Code-Gen Orchestrator
 *
 * Runs all four independent C# code-generation scripts in parallel:
 *   • generate-css-classes.mjs    → Constants/Classes.g.cs
 *   • generate-asset-paths.mjs    → Constants/Assets.g.cs + manifest.json
 *   • generate-localization-keys.mjs → Constants/Loc.g.cs
 *   • generate-icon-constants.mjs → Constants/Icons.g.cs
 *
 * All four read disjoint source files and write to disjoint output files.
 * No cross-dependencies → safe to run fully in parallel.
 *
 * Expected wall-clock: max(≈61, ≈69, ≈50, ≈88) ms ≈ 90ms
 * vs sequential:  61 + 69 + 50 + 88 ≈ 268ms  (~3x speedup)
 */

import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { performance } from 'node:perf_hooks';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function msLabel(ms) {
  return ms < 1000 ? `${Math.round(ms)}ms` : `${(ms / 1000).toFixed(2)}s`;
}

function runScript(label, scriptName, args = []) {
  return new Promise((resolve, reject) => {
    const scriptPath = path.resolve(__dirname, scriptName);
    const start = performance.now();
    const proc = spawn(process.execPath, [scriptPath, ...args], {
      stdio: ['ignore', 'pipe', 'pipe'],
      env: process.env,
    });

    const prefix = `  [${label}] `;
    proc.stdout.on('data', (chunk) =>
      chunk.toString().split('\n').filter(Boolean).forEach((line) =>
        process.stdout.write(`${prefix}${line}\n`)
      )
    );
    proc.stderr.on('data', (chunk) =>
      chunk.toString().split('\n').filter(Boolean).forEach((line) =>
        process.stderr.write(`${prefix}${line}\n`)
      )
    );

    proc.on('close', (code) => {
      const elapsed = performance.now() - start;
      if (code === 0) resolve(elapsed);
      else reject(new Error(`${scriptName} exited with code ${code}`));
    });
    proc.on('error', reject);
  });
}

async function main() {
  const t0 = performance.now();
  console.log('⚙️  [Code-Gen Orchestrator] Parallel C# constants generation starting...\n');

  const extraArgs = process.argv.slice(2);

  const results = await Promise.allSettled([
    runScript('classes', 'generate-css-classes.mjs', extraArgs),
    runScript('assets ', 'generate-asset-paths.mjs', extraArgs),
    runScript('loc    ', 'generate-localization-keys.mjs', extraArgs),
    runScript('icons  ', 'generate-icon-constants.mjs', extraArgs),
  ]);

  // Surface first error if any
  const failed = results.find((r) => r.status === 'rejected');
  if (failed) throw failed.reason;

  const [classesMs, assetsMs, locMs, iconsMs] = results.map((r) => r.value ?? 0);
  const totalMs = performance.now() - t0;

  const pad = (s) => String(s).padEnd(10);
  console.log(`
┌────────────────────────────────────────────┐
│  ⚙️  Code-Gen Summary (all parallel)        │
├────────────────────────────────────────────┤
│  generate-css-classes     : ${pad(msLabel(classesMs))}     │
│  generate-asset-paths     : ${pad(msLabel(assetsMs))}     │
│  generate-localization-keys: ${pad(msLabel(locMs))}     │
│  generate-icon-constants  : ${pad(msLabel(iconsMs))}     │
├────────────────────────────────────────────┤
│  ⚡ Total wall-clock       : ${pad(msLabel(totalMs))}     │
└────────────────────────────────────────────┘
`);
}

main().catch((err) => {
  console.error('\n❌ Code-gen orchestrator failed:', err.message ?? err);
  process.exit(1);
});
