# Verification and Phase Handoff

## Verify the Result

1. Run the checks explicitly required by the phase, applicable `AGENTS.md`
   files, and `docs/sdd/standards/testing-rules.md` when they are relevant to
   the changed files.
2. Start with the narrowest check: targeted unit, integration, or API tests.
   For phase `05`, always run:

   ```powershell
   dotnet restore KedrStore.sln
   dotnet build KedrStore.sln --no-restore
   dotnet test KedrStore.sln --no-build
   ```

3. If a command cannot be run or fails, record the exact command, failure
   point, and whether the issue was introduced by the current change or already
   existed.
4. Review `git diff` for your own changes. They must match the current phase;
   do not change or discard unrelated working-tree changes.

## Completion Condition

A phase is complete only when all its checkbox tasks are closed, its checkpoint
is satisfied, blockers are absent or have an explicit user decision, and the
code, contracts, and documentation do not contradict each other. For phase
`05`, `checklist/delivery-readiness.md` must also be completed and a delivery
report prepared.

## Final Report Format

1. **Completed** — task IDs and a short result.
2. **Changed files** — only changes from the current phase.
3. **Verification** — commands, results, and manual scenarios.
4. **Not verified / blockers** — the exact reason; state that there are none if
   applicable.
5. **Risks / manual verification** — only real remaining items.
6. **Status** — `Phase <number> is complete. Waiting for the command “Move to phase <next number>.”`

After reporting, do not start the next phase or make further changes until the
user explicitly names its number.
