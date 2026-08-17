# Admin order and 1C synchronization — checklist: readiness for delivery

- [ ] Domain invariants and type boundaries are tested.
- [ ] Persistence constraints, migration generation, empty-database application, and rollout/rollback are verified.
- [ ] The API conforms to `docs/sdd/contracts/sales/admin-order-one-c-sync.openapi.yaml`, including errors, authorization, and idempotency.
- [ ] SOAP adapter mapping covers acceptance, business failure, transport failure, duplicate retry, and sanitized diagnostics.
- [ ] Scheduled delivery covers time-window deferral, batch limiting, retry backoff, maximum attempts, and concurrent claim safety.
- [ ] `dotnet restore KedrStore.sln`, `dotnet build KedrStore.sln --no-restore`, and `dotnet test KedrStore.sln --no-build` have run or their failures are recorded.
- [ ] Documentation, OpenAPI registry, implementation tasks, and code agree; residual risks and operational configuration are recorded in the delivery report.
