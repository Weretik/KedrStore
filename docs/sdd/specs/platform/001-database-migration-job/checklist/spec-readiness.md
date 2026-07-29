# Database migration job — checklist: specification readiness

- [x] Production failure and migration-history mismatch are evidenced by Cloud Run logs and `__EFMigrationsHistory` output.
- [x] The legacy Identity migration IDs that must remain compatible are known.
- [x] No public HTTP contract is in scope; the internal CLI and deployment contracts are documented.
- [x] Rollout and rollback require an explicit Cloud SQL backup.
- [x] Every stated requirement is covered by a task ID.
- [x] The dedicated root Jobs service is explicitly in scope.
