import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawn } from 'node:child_process';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const assetsRoot = path.resolve(__dirname, '..');
const repoFrontendRoot = path.resolve(assetsRoot, '../../../../');
const previewGeneratorDir = path.resolve(assetsRoot, '../../../Tools/PreviewGenerator');
const previewGeneratorDll = path.resolve(previewGeneratorDir, 'bin/Release/net10.0/PreviewGenerator.dll');
const previewGeneratorCsproj = path.resolve(previewGeneratorDir, 'PreviewGenerator.csproj');
const targetPreviewDir = path.resolve(assetsRoot, 'wwwroot/preview');

const isRebuildRequested = process.argv.includes('--rebuild') || process.argv.includes('-r');

function runProcess(cmd, args, cwd) {
  return new Promise((resolve, reject) => {
    const proc = spawn(cmd, args, { cwd, stdio: 'inherit', shell: false });
    proc.on('close', (code) => {
      if (code === 0) resolve();
      else reject(new Error(`Process exited with code ${code}`));
    });
    proc.on('error', reject);
  });
}

async function generatePreview() {
  const startTime = performance.now();
  console.log('⚡ [Preview Generator] Launching Blazor Headless HtmlRenderer...');

  const dllExists = fs.existsSync(previewGeneratorDll);

  if (!dllExists || isRebuildRequested) {
    console.log('🔨 Compiling PreviewGenerator project (-c Release)...');
    await runProcess('dotnet', ['build', previewGeneratorCsproj, '-c', 'Release', '--verbosity', 'minimal'], previewGeneratorDir);
  }

  console.log(`🚀 Executing PreviewGenerator (${path.basename(previewGeneratorDll)})...`);
  await runProcess('dotnet', [previewGeneratorDll, '--out-dir', targetPreviewDir], previewGeneratorDir);

  const durationMs = Math.round(performance.now() - startTime);
  const generatedHtmlFiles = fs.readdirSync(targetPreviewDir).filter(f => f.endsWith('.html'));

  console.log(`\n✨ [Preview Generation Complete in ${durationMs}ms]`);
  console.log(`   📄 Generated pages: ${generatedHtmlFiles.length} HTML files in wwwroot/preview/\n`);
}

generatePreview().catch((err) => {
  console.error('❌ Failed to generate preview pages:', err.message);
  process.exit(1);
});
