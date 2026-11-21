/**
 * generate-analytics.mjs — Parallel Analytics Orchestrator
 *
 * Runs static Razor/C# usage analysis and live Blazor WASM layout audit (index.html#/*)
 * in parallel without intermediate preview HTML files.
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

  const extraArgs = process.argv.slice(2);
  let usageMs = 0;
  let blueprintsMs = 0;

  const [usageResult, blueprintsResult] = await Promise.allSettled([
    runScript('usage     ', 'generate-usage.mjs', extraArgs).then((ms) => { usageMs = ms; }),
    runScript('blueprints', 'generate-blueprints.mjs', extraArgs).then((ms) => { blueprintsMs = ms; }),
  ]);

  if (usageResult.status === 'rejected') throw usageResult.reason;
  if (blueprintsResult.status === 'rejected') throw blueprintsResult.reason;

  const totalMs = performance.now() - t0;
  const pad = (s) => String(s).padEnd(12);
  console.log(`
┌──────────────────────────────────────────────────┐
│  📊 Analytics Pipeline Summary                   │
├──────────────────────────────────────────────────┤
│  generate-usage      : ${pad(msLabel(usageMs))}(parallel)   │
│  generate-blueprints : ${pad(msLabel(blueprintsMs))}(parallel)   │
├──────────────────────────────────────────────────┤
│  ⚡ Total wall-clock  : ${pad(msLabel(totalMs))}             │
└──────────────────────────────────────────────────┘
`);
}

main().catch((err) => {
  console.error('\n❌ Analytics orchestrator failed:', err.message ?? err);
  process.exit(1);
});
