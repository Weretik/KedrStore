# Documentation: instructions for AI

These instructions apply to changes in `docs/`. Follow the root `AGENTS.md` first; this file only refines documentation work.

## Navigation

1. For backend architecture, read `sdd/architecture/README.md`.
2. For stable rules, read `sdd/standards/README.md` and choose only the relevant rules.
3. For a new feature or migration, read `sdd/specs/_templates/README.md` and create or update the feature specification before code.
4. For startup, configuration, migrations, diagnostics, or jobs, read `sdd/operations/README.md`.
5. For stable catalog and door-industry terms, read `product/glossary.md`.

## Documentation rules

- Keep documentation as Markdown alongside code, and update it in the same change set as the behavior it describes.
- `architecture/` and `standards/` contain stable rules; feature-specific decisions belong in `specs/<module>/<NNN>-<feature>/`.
- One responsibility requires one document. Do not create empty, duplicate, or monolithic files.
- Keep the API contract in `contracts/`; agree a versioned OpenAPI YAML before frontend handoff.
- For implementation, use `tasks/`: complete phases in order, mark task IDs, and record verification results.
- Verify relative Markdown links after moving or renaming files.
