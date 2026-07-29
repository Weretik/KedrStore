# Database migration job — checklist: delivery readiness

- [ ] Historical migration lineage and snapshot are restored without altering applied IDs.
- [ ] Forward migration SQL is reviewed against a disposable production-like database.
- [ ] `Host.Jobs --job=migrate` is covered by focused tests and is idempotent.
- [ ] `Host.Api` starts without applying migrations or seeders.
- [ ] Cloud Run migration job runs before API deployment and blocks on failure.
- [ ] Backup, rollout logs, restore procedure, and known limitations are recorded.
