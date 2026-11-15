# ADR-0012: A code folder holds at most six files

- Status: accepted
- Date: 2026-11-14 (owner's rule)
- Owner: Виталий (project owner)

## Context

After the restructure of [ADR-0004](0004-contracts-and-business-folders.md), some folders still held fifteen to thirty files. A folder that large hides its meaning: a reader cannot see which files belong together, and new files land there by habit.

## Alternatives considered

- No limit, rely on review: rejected; folders grew back.
- Split by kind of class (`Entities/`, `Dtos/`): rejected by ADR-0004.

## Decision

- A folder in `src/` and `tests/` holds at most six code files. When it would hold more, it splits by meaning into named subfolders of at least two files each.
- No empty folders, no wrapper folders (no files and one subfolder), no new one-file folders.
- Only generated code is exempt (EF `Migrations/`, `Generated/`).
- The architecture test `CodeFolders_HoldAtMostSixFiles_AndAreNeitherEmptyNorWrappers` enforces it; every audit lists violations.

## Why

- The folder tree reads like a table of contents of the business.
- The rule is mechanical, so it is checked by a test, not by memory.

## Trade-offs

- More folders and longer namespaces.
- Splitting an existing folder changes namespaces in many files.

## Sources and checks

- [Module rules: the folder axiom](../../standards/module-rules.md#the-folder-axiom-at-most-six-files), `tests/ArchitectureTests/Folders/FolderStructureTests.cs`.
- The same rule applies to BreviERP and to the frontends.
