# Verification and phase handoff

## Verify the result

1. Run checks explicitly required by the phase, applicable `AGENTS.md` files,
   and `docs/sdd/standards/testing-rules.md` when relevant to the changed files.
2. Start with the narrowest relevant check: targeted unit, integration, or API
   tests. For build/test subphase `05.N`, run the agreed CLI commands, for
   example:

   ```powershell
   dotnet restore KedrStore.sln
   dotnet build KedrStore.sln --no-restore
   dotnet test KedrStore.sln --no-build
   ```

3. If a command cannot run or fails, record the exact command, point of
   failure, and whether the issue was introduced by the current change or was
   already present.
4. Review `git diff` for your own changes. They must match the current phase;
   do not change or discard unrelated working-tree changes.

## Completion condition

The current file is complete only when all its checkbox tasks are closed, its
checkpoint is satisfied, blockers are absent or have an explicit user decision,
and code, contracts, and documentation agree. Subphase `05.N` additionally
requires `checklist/delivery-readiness.md` and a delivery report.

## Final report format

1. **Completed** — task IDs and a concise result.
2. **Changed files** — only changes from the current phase.
3. **Verification** — commands, results, and manual scenarios.
4. **Not verified / blockers** — the exact reason; state when there are none.
5. **Risks / manual verification** — only real remaining items.
6. **Status** — `Phase file <path> is complete. Waiting for a command naming the next exact phase file.`

After the report, do not start the next phase file or make new changes until the
user explicitly names its number or path.
