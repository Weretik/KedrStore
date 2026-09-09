# Documentation: instructions for AI

These instructions apply to changes in `docs/`. Follow any repository-level
`AGENTS.md` first; this file refines documentation work.

## Navigation

1. For backend architecture, read `sdd/architecture/README.md`.
2. For stable rules, read `sdd/standards/README.md` and select only the
   relevant rules. Before creating or changing an entity identifier, read
   `sdd/standards/identifier-strategy.md`.
3. For a new feature or migration, read `sdd/specs/_templates/README.md` and
   create or update the feature specification before code.
4. For startup, configuration, migrations, diagnostics, or jobs, read
   `sdd/operations/README.md`.
5. For stable catalog and door-industry terms, read `product/glossary.md`.

## AI execution scope

When the user authorizes implementation of a feature, phase, scenario, or
ordered task set, continue through all ready in-scope tasks without requesting
a separate command for every task file. Respect task dependencies and
checkpoints from `sdd/specs/_templates/ai-feature-workflow/`. Stop only at the
authorized boundary or a documented blocker.

## Documentation rules

- Keep documentation as Markdown alongside code, and update it in the same
  change set as the behavior it describes.
- `architecture/` and `standards/` contain stable rules; feature-specific
  decisions belong in `specs/<module>/<NNN>-<feature>/`.
- One responsibility requires one document. Do not create empty, duplicate, or
  monolithic files.
- Keep API contracts in `contracts/` and follow `sdd/contracts/README.md`.
- For feature specification and implementation, follow the linked SDD template,
  testing rules, and AI workflow instead of duplicating their instructions here.
- Verify relative Markdown links after moving or renaming files.
