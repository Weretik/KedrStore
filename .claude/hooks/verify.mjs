#!/usr/bin/env node
// Stop hook: before Claude says it is done, compiles the projects touched by uncommitted code changes.
// Backend: `dotnet build` of each changed project. Frontend: Nx `typecheck` of the affected projects.
// A failure sends the errors back to Claude, which keeps working. Unchanged code is not checked twice.
import { execFileSync, spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { existsSync, readFileSync, readdirSync, writeFileSync } from 'node:fs';
import path from 'node:path';

let data = '';
for await (const chunk of process.stdin) data += chunk;
const input = JSON.parse(data || '{}');
const root = process.env.CLAUDE_PROJECT_DIR || input.cwd || process.cwd();

const changed = git(['status', '--porcelain=v1', '-uall'])
  .split('\n')
  .filter((line) => line && line[0] !== 'D' && line[1] !== 'D')
  .map((line) => line.slice(3).split(' -> ').pop().replace(/^"|"$/g, ''));

const isDotnet = readdirSync(root).some((f) => f.endsWith('.sln'));
const isNx = existsSync(path.join(root, 'nx.json'));
const code = changed.filter((f) =>
  isDotnet
    ? /^(src|tests)\/.*\.(cs|csproj)$/.test(f)
    : isNx && /^(apps|libs)\/.*\.(ts|tsx|mts|cts)$/.test(f),
);
if (code.length === 0) process.exit(0);

const stampFile = path.resolve(root, git(['rev-parse', '--git-dir']).trim(), 'claude-verify.json');
const hash = createHash('sha1');
for (const file of code.sort()) {
  hash.update(file);
  try {
    hash.update(readFileSync(path.join(root, file)));
  } catch {
    // A file removed between status and read only changes the hash.
  }
}
const fingerprint = hash.digest('hex');
const stamp = readStamp();
// Same code as the last check: it already passed, or Claude already saw the errors.
if (stamp?.fingerprint === fingerprint) process.exit(0);

const result = isDotnet ? buildDotnet(code) : typecheckNx(code);
writeFileSync(stampFile, JSON.stringify({ fingerprint, ok: result.ok }));
if (!result.ok) {
  const tail = result.output.split('\n').filter(Boolean).slice(-60).join('\n');
  process.stdout.write(
    JSON.stringify({
      decision: 'block',
      reason: `${result.label} failed for your uncommitted changes. Fix these errors before finishing:\n${tail}`,
    }),
  );
}

function buildDotnet(files) {
  const projects = [...new Set(files.map(nearestProject).filter(Boolean))];
  let output = '';
  for (const project of projects) {
    const run = spawnSync('dotnet', ['build', project, '--nologo', '-v', 'q', '-clp:ErrorsOnly'], {
      cwd: root,
      encoding: 'utf8',
      env: { ...process.env, DOTNET_NOLOGO: '1', DOTNET_CLI_TELEMETRY_OPTOUT: '1' },
    });
    if (run.error) return { ok: true, output: '', label: 'dotnet build' }; // dotnet is not installed here.
    if (run.status !== 0) output += `${project}\n${run.stdout}${run.stderr}\n`;
  }
  return { ok: output === '', output, label: 'dotnet build' };
}

function nearestProject(file) {
  let dir = path.dirname(path.join(root, file));
  while (dir.startsWith(root) && dir !== root) {
    const project = readdirSync(dir).find((f) => f.endsWith('.csproj'));
    if (project) return path.relative(root, path.join(dir, project));
    dir = path.dirname(dir);
  }
  return null;
}

function typecheckNx(files) {
  const nx = packageBin('nx', 'nx');
  if (!nx) return { ok: true, output: '', label: 'nx typecheck' };
  const run = spawnSync(
    process.execPath,
    [nx, 'affected', '-t', 'typecheck', `--files=${files.join(',')}`, '--outputStyle=static'],
    {
      cwd: root,
      encoding: 'utf8',
      maxBuffer: 64 * 1024 * 1024,
      env: { ...process.env, FORCE_COLOR: '0', NX_TUI: 'false' },
    },
  );
  if (run.error) return { ok: true, output: '', label: 'nx typecheck' };
  return { ok: run.status === 0, output: `${run.stdout}${run.stderr}`, label: 'nx typecheck' };
}

function packageBin(name, bin) {
  const manifest = path.join(root, 'node_modules', name, 'package.json');
  if (!existsSync(manifest)) return null;
  const bins = JSON.parse(readFileSync(manifest, 'utf8')).bin;
  const file = typeof bins === 'string' ? bins : bins?.[bin];
  return file ? path.join(path.dirname(manifest), file) : null;
}

function readStamp() {
  try {
    return JSON.parse(readFileSync(stampFile, 'utf8'));
  } catch {
    return null;
  }
}

function git(args) {
  return execFileSync('git', args, { cwd: root, encoding: 'utf8' });
}
