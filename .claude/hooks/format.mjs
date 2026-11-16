#!/usr/bin/env node
// PostToolUse formatter: formats the file Claude just wrote, so diffs keep the repository style.
// .cs files get `dotnet format whitespace`; web files get Prettier. Failures never block the edit.
import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync, readdirSync } from 'node:fs';
import path from 'node:path';

let data = '';
for await (const chunk of process.stdin) data += chunk;
const input = JSON.parse(data || '{}');
const root = process.env.CLAUDE_PROJECT_DIR || input.cwd || process.cwd();
const filePath = input.tool_input?.file_path;

if (filePath && existsSync(filePath)) {
  const relative = path.relative(root, path.resolve(root, filePath));
  const inside = !relative.startsWith('..') && !path.isAbsolute(relative);
  const prettier = packageBin('prettier', 'prettier');
  try {
    if (inside && /\.cs$/i.test(relative) && readdirSync(root).some((f) => f.endsWith('.sln'))) {
      run('dotnet', ['format', 'whitespace', root, '--folder', '--include', relative]);
    } else if (
      inside &&
      /\.(ts|tsx|mts|cts|js|jsx|mjs|cjs|json|css|scss|html|md)$/i.test(relative) &&
      prettier
    ) {
      run(process.execPath, [
        prettier,
        '--write',
        '--log-level',
        'warn',
        '--ignore-unknown',
        relative,
      ]);
    }
  } catch {
    // Formatting is a convenience; the edit stands even if the formatter is missing or fails.
  }
}

function packageBin(name, bin) {
  const manifest = path.join(root, 'node_modules', name, 'package.json');
  if (!existsSync(manifest)) return null;
  const bins = JSON.parse(readFileSync(manifest, 'utf8')).bin;
  const file = typeof bins === 'string' ? bins : bins?.[bin];
  return file ? path.join(path.dirname(manifest), file) : null;
}

function run(command, args) {
  execFileSync(command, args, { cwd: root, stdio: 'ignore', timeout: 50_000 });
}
