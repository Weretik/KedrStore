# Phases of feature implementation

The feature follows the standard `00 → 05` sequence. Each phase has concrete, ordered task files with ownership boundaries, exact target paths, dependencies, and checkpoints.

- [00 — Readiness](00-readiness.md)
- [01 — Domain planning](01-domain.md)
- [02 — Infrastructure planning](02-infrastructure.md)
- [03 — Application planning](03-application.md)
- [04 — API planning](04-api.md)
- [05 — Background delivery job](05-jobs.md)
- [06 — Verification](06-verification.md)

## Existing concrete subphases

- [00.1 — Confirm the implementation baseline](readiness/00.1-scope-contracts.md)
- [00.2 — Discover CLI and generators](readiness/00.2-tooling.md)

API subphases follow the required order: controller → HTTP behavior/security → OpenAPI → API tests.
