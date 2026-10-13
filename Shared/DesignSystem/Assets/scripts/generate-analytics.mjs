/**
 * generate-analytics.mjs — Parallel Analytics Orchestrator
 *
 * Dependency graph:
 *   generate-usage.mjs     ──────────────────────────────────────► (reads only .razor/.cs)
 *   generate-preview.mjs   ──────────► generate-blueprints.mjs    (reads preview/*.html)
 *
 * Wall-clock strategy:
 *   Phase 1 (parallel): generate-usage + generate-preview
 *   Phase 2 (serial):   generate-blueprints (after preview ready)
 *
 * Expected: max(usage≈160ms, preview≈600ms) + blueprints≈1100ms ≈ 1.7s
 * vs sequential: 160 + 600 + 1100 ≈ 1.86s  (modest gain here)
 * But for CI/watch loops the gain compounds; and usage runs free.
 */

import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { performance } from 'node:perf_hooks';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const scriptsDir = __dirname;

function msLabel(ms) {
  return ms < 1000 ? `${Math.round(ms)}ms` : `${(ms / 1000).toFixed(2)}s`;
}

/**
 * Run a Node.js script as a child process, streaming its output with a prefix label.
 * Resolves with elapsed time in ms, rejects on non-zero exit.
 */
function runScript(label, scriptName, args = []) {
  return new Promise((resolve, reject) => {
    const scriptPath = path.resolve(scriptsDir, scriptName);
    const start = performance.now();
    const proc = spawn(process.execPath, [scriptPath, ...args], {
      stdio: ['ignore', 'pipe', 'pipe'],
      env: process.env,
    });

    const prefix = `  [${label}] `;
    proc.stdout.on('data', (chunk) => {
      chunk.toString().split('\n').filter(Boolean).forEach((line) =>
        process.stdout.write(`${prefix}${line}\n`)
      );
    });
    proc.stderr.on('data', (chunk) => {
      chunk.toString().split('\n').filter(Boolean).forEach((line) =>
        process.stderr.write(`${prefix}${line}\n`)
      );
    });

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
  console.log('🚀 [Analytics Orchestrator] Parallel pipeline starting...\n');

  const extraArgs = process.argv.slice(2); // pass-through flags (e.g. --static-only)

  // ── Phase 1: usage and preview in parallel ──────────────────────────────────
  let usageMs = 0;
  let previewMs = 0;

  const [usageResult, previewResult] = await Promise.allSettled([
    runScript('usage  ', 'generate-usage.mjs', extraArgs).then((ms) => { usageMs = ms; }),
    runScript('preview', 'generate-preview.mjs', extraArgs).then((ms) => { previewMs = ms; }),
  ]);

  // Surface errors immediately
  if (usageResult.status === 'rejected') throw usageResult.reason;
  if (previewResult.status === 'rejected') throw previewResult.reason;

  // ── Phase 2: blueprints (depends on preview output) ─────────────────────────
  const blueprintsMs = await runScript('blueprints', 'generate-blueprints.mjs', extraArgs);

  const totalMs = performance.now() - t0;

  // Summary
  const pad = (s) => String(s).padEnd(12);
  console.log(`
┌──────────────────────────────────────────────────┐
│  📊 Analytics Pipeline Summary                   │
├──────────────────────────────────────────────────┤
│  generate-usage      : ${pad(msLabel(usageMs))}(parallel)   │
│  generate-preview    : ${pad(msLabel(previewMs))}(parallel)   │
│  generate-blueprints : ${pad(msLabel(blueprintsMs))}(sequential) │
├──────────────────────────────────────────────────┤
│  ⚡ Total wall-clock  : ${pad(msLabel(totalMs))}             │
└──────────────────────────────────────────────────┘
`);
}

main().catch((err) => {
  console.error('\n❌ Analytics orchestrator failed:', err.message ?? err);
  process.exit(1);
});
