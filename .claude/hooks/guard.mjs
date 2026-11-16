#!/usr/bin/env node
// PreToolUse guard: blocks reading or editing secret files and dangerous shell commands.
// Claude Code passes the tool call as JSON on stdin; a "deny" decision on stdout stops the call.
import { execFileSync } from 'node:child_process';
import path from 'node:path';

const input = JSON.parse(await readStdin());
const root = process.env.CLAUDE_PROJECT_DIR || input.cwd || process.cwd();
const tool = input.tool_name ?? '';
const toolInput = input.tool_input ?? {};

const reason = ['Bash', 'PowerShell'].includes(tool)
  ? checkCommand(String(toolInput.command ?? ''))
  : checkFile(toolInput.file_path ?? toolInput.notebook_path);

if (reason) {
  process.stdout.write(
    JSON.stringify({
      hookSpecificOutput: {
        hookEventName: 'PreToolUse',
        permissionDecision: 'deny',
        permissionDecisionReason: `${reason} Ask the user to do this step themselves if it is really needed.`,
      },
    }),
  );
}

function checkFile(filePath) {
  if (!filePath) return null;
  return isSecretPath(String(filePath))
    ? `Blocked: ${filePath} may hold secrets (passwords, keys, tokens).`
    : null;
}

function checkCommand(command) {
  const flat = command.replace(/\s+/g, ' ');
  for (const part of flat.split(/&&|\|\||;|\||\n/)) {
    const cmd = part.trim();
    if (/^git\b.*\bpush\b/.test(cmd)) {
      if (/\s(--force(?!-with-lease)\b|-[a-zA-Z]*f\b|\+\S)/.test(cmd))
        return 'Blocked: force push rewrites remote history.';
      if (
        /\s(\S+:)?(refs\/heads\/)?(main|master)$|\s(\S+:)?(refs\/heads\/)?(main|master)\s/.test(cmd)
      )
        return 'Blocked: push straight to main/master; push a branch and open a pull request.';
      if (
        !/\s\S+\s+\S+$/.test(cmd.replace(/\s-{1,2}[a-zA-Z-]+(=\S+)?/g, '')) &&
        ['main', 'master'].includes(currentBranch())
      )
        return 'Blocked: the current branch is main/master; push a branch and open a pull request.';
    }
    if (
      /\bdotnet\b.*\bef\b.*\bdatabase\s+update\b/.test(cmd) &&
      /--connection|production/i.test(cmd)
    )
      return 'Blocked: production migrations run only through the deploy pipeline.';
    const removal = recursiveRemovalTargets(cmd);
    if (removal && !removal.every(isDisposable))
      return `Blocked: recursive force delete of ${removal.join(' ') || 'an unknown path'}.`;
    for (const token of cmd
      .split(' ')
      .map((t) => t.replace(/^['"]|['"]$/g, '').replace(/^[<>]+/, ''))) {
      if (token && /[./\\]/.test(token) && isSecretPath(token))
        return `Blocked: ${token} may hold secrets.`;
    }
  }
  return null;
}

function recursiveRemovalTargets(cmd) {
  const rm = cmd.match(/^(?:sudo\s+)?rm\s+(.*)$/);
  if (rm) {
    const args = rm[1].split(' ');
    const flags = args.filter((a) => a.startsWith('-')).join('');
    const recursive = /-[a-zA-Z]*r|--recursive/i.test(flags);
    const force = /-[a-zA-Z]*f|--force/.test(flags);
    return recursive && force ? args.filter((a) => a && !a.startsWith('-')) : null;
  }
  const ps = cmd.match(/^(?:Remove-Item|rd|rmdir)\s+(.*)$/i);
  if (ps && /-Recurse|\/s\b/i.test(ps[1])) {
    return ps[1].split(' ').filter((a) => a && !/^[-/]/.test(a) && !/^-?Path$/i.test(a));
  }
  return null;
}

function isDisposable(target) {
  const clean = target.replace(/^['"]|['"]$/g, '').replace(/[\\/]+$/, '');
  if (!clean || clean.includes('*') || clean.includes('..')) return false;
  const name = clean.split(/[\\/]/).pop();
  return /^(node_modules|dist|bin|obj|out|build|coverage|tmp|\.nx|\.next|\.open-next|\.angular|\.vite|TestResults|test-results|playwright-report)$/.test(
    name,
  );
}

function isSecretPath(filePath) {
  const p = filePath.replace(/\\/g, '/').toLowerCase();
  const name = p.split('/').pop();
  if (/(^|\/)\.ssh\/|usersecrets\/|\/\.aws\/|\/gcloud\//.test(p)) return true;
  if (/^(secrets\.json|\.dev\.vars.*|application_default_credentials\.json)$/.test(name))
    return true;
  if (/\.(pem|key|pfx|p12)$/.test(name) || /(credentials|service-?account)[^/]*\.json$/.test(name))
    return true;
  // .env and appsettings files committed to git hold only public settings; untracked ones hold secrets.
  if (/^\.env(\..+)?$|\.env$/.test(name) && !/\.(example|sample|template)$/.test(name))
    return !isTracked(filePath);
  if (/^appsettings.*\.json$/.test(name)) return !isTracked(filePath);
  return false;
}

function isTracked(filePath) {
  const absolute = path.resolve(root, filePath);
  const relative = path.relative(root, absolute);
  if (relative.startsWith('..') || path.isAbsolute(relative)) return false;
  try {
    execFileSync('git', ['ls-files', '--error-unmatch', '--', relative], {
      cwd: root,
      stdio: 'ignore',
    });
    return true;
  } catch {
    return false;
  }
}

function currentBranch() {
  try {
    return execFileSync('git', ['rev-parse', '--abbrev-ref', 'HEAD'], {
      cwd: root,
      encoding: 'utf8',
    }).trim();
  } catch {
    return '';
  }
}

async function readStdin() {
  let data = '';
  for await (const chunk of process.stdin) data += chunk;
  return data || '{}';
}
