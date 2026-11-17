// Tests for the PreToolUse guard: run with `node --test .claude/hooks/guard.test.mjs`.
// Each case feeds a tool call to guard.mjs inside a throwaway git repository and checks whether it is denied.
import { execFileSync, spawnSync } from 'node:child_process';
import { mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { after, before, describe, test } from 'node:test';
import assert from 'node:assert/strict';

const guard = path.join(import.meta.dirname, 'guard.mjs');
let repo;

before(() => {
  repo = mkdtempSync(path.join(tmpdir(), 'guard-'));
  const git = (...args) => execFileSync('git', args, { cwd: repo, stdio: 'ignore' });
  git('init', '-q', '-b', 'feature');
  writeFileSync(path.join(repo, 'appsettings.json'), '{}');
  writeFileSync(path.join(repo, '.env.example'), 'KEY=');
  git('add', '.');
  git('-c', 'user.name=t', '-c', 'user.email=t@t', 'commit', '-q', '-m', 'init');
});

after(() => rmSync(repo, { recursive: true, force: true }));

function denied(tool, toolInput, branch = 'feature') {
  execFileSync('git', ['checkout', '-q', '-B', branch], { cwd: repo, stdio: 'ignore' });
  const run = spawnSync(process.execPath, [guard], {
    input: JSON.stringify({ tool_name: tool, tool_input: toolInput, cwd: repo }),
    env: { ...process.env, CLAUDE_PROJECT_DIR: repo },
    encoding: 'utf8',
  });
  assert.equal(run.status, 0, run.stderr);
  return run.stdout.includes('"deny"');
}

const bash = (command, branch) => denied('Bash', { command }, branch);
const read = (file_path) => denied('Read', { file_path });

describe('shell commands', () => {
  test('force push is blocked, --force-with-lease is not', () => {
    assert.ok(bash('git push --force origin feature'));
    assert.ok(bash('git push -f origin feature'));
    assert.ok(bash('git push origin +feature'));
    assert.ok(!bash('git push --force-with-lease origin feature'));
  });

  test('push to main or master is blocked', () => {
    assert.ok(bash('git push origin main'));
    assert.ok(bash('git push origin HEAD:refs/heads/master'));
    assert.ok(bash('git push', 'main'));
    assert.ok(!bash('git push -u origin feature'));
    assert.ok(!bash('git push'));
  });

  test('recursive force delete is allowed only for build output', () => {
    assert.ok(!bash('rm -rf node_modules dist'));
    assert.ok(!bash('rm -rf src/Host/bin'));
    assert.ok(bash('rm -rf src'));
    assert.ok(bash('rm -rf /'));
    assert.ok(bash('rm -rf node_modules/../src'));
    assert.ok(bash('cd x && rm -fr *'));
    assert.ok(denied('PowerShell', { command: 'Remove-Item -Recurse -Force src' }));
    assert.ok(!bash('rm -r src/old'));
  });

  test('production migrations are blocked', () => {
    assert.ok(bash('dotnet ef database update --connection "Host=prod"'));
    assert.ok(!bash('dotnet ef migrations add AddOrders'));
  });

  test('commands that touch secret files are blocked', () => {
    assert.ok(bash('cat .env'));
    assert.ok(bash('cat ~/.ssh/id_rsa'));
    assert.ok(bash('cp key.pem /tmp'));
    assert.ok(!bash('cat .env.example'));
    assert.ok(!bash('npm test'));
  });
});

describe('file tools', () => {
  test('secret files are blocked', () => {
    assert.ok(read('.env'));
    assert.ok(read('/home/u/.ssh/id_ed25519'));
    assert.ok(read('appsettings.Development.json'));
    assert.ok(read('service-account.json'));
    assert.ok(read('.dev.vars'));
  });

  test('tracked settings and ordinary files are allowed', () => {
    assert.ok(!read('appsettings.json'));
    assert.ok(!read('.env.example'));
    assert.ok(!read('src/app.ts'));
    assert.ok(!denied('Edit', { file_path: 'README.md' }));
  });
});
