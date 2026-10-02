# Phase 02 — Infrastructure task planning

- [x] Execute [EN-001 — persistence migration](infrastructure/02.1-persistence-migration.md).
- [x] Execute [TS-002 — persistence integrity](infrastructure/02.2-persistence-integrity.md).
- [x] Execute [TS-003 — CDN probe](infrastructure/02.3-cdn-probe.md).

## Checkpoint

PostgreSQL owns the agreed relationship and index, and the HTTP adapter
classifies CDN responses without downloading complete images or escaping the
allowed host.
