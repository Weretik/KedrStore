# Database migration job — domain and ownership

No business-domain model changes are required.

`IDatabaseMigrator` remains the infrastructure abstraction that owns applying migrations for a specific EF `DbContext`. `Host.Jobs` owns process orchestration only: it resolves the registered migrators, invokes them in a deterministic order, logs progress, and returns an exit code.

`Host.Api` owns HTTP serving and must not own schema initialization. This keeps Cloud Run readiness independent from database migration duration or failures.
