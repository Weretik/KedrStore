# Missing product photos — task graph

All tasks were executed after implementation authorization. Evidence is
recorded in each detailed task file.

```text
TS-001 photo-check domain state
   └── EN-001 migration and backfill
          ├── TS-002 persistence integrity
          ├── TS-004 job/import orchestration ──┐
TS-003 CDN probe adapter ────────────────┘      │
                                                ├── TS-005 unavailable-photo query
                                                │      └── TS-006 HTTP contract/endpoint
                                                └── EN-002 Host.Jobs + Cloud Run wiring
TS-001..TS-006 + EN-001..EN-002 ── TS-007 verification and delivery
```

| ID | Responsibility | Depends on | Detailed task |
| --- | --- | --- | --- |
| TS-001 | Photo-check state and URL invalidation | none | [Domain state](domain/01.1-photo-check-state.md) |
| EN-001 | EF mapping, generated migration, and active-product backfill | TS-001 | [Persistence enabler](infrastructure/02.1-persistence-migration.md) |
| TS-002 | PostgreSQL lifecycle and stale-write integrity | EN-001 | [Persistence behavior](infrastructure/02.2-persistence-integrity.md) |
| TS-003 | Bounded CDN HTTP probe | TS-001 | [CDN adapter](infrastructure/02.3-cdn-probe.md) |
| TS-004 | Import reconciliation and one-shot check job | TS-001, EN-001, TS-003 | [Job orchestration](application/03.1-check-product-photos.md) |
| TS-005 | Paged unavailable-photo read use case | EN-001 | [List query](application/03.2-get-unavailable-products.md) |
| TS-006 | OpenAPI and anonymous HTTP endpoint | TS-005 | [API endpoint](api/04.1-missing-photo-endpoint.md) |
| EN-002 | Host command, operations docs, and Cloud Run definition | TS-004 | [Deployment wiring](enablers/EN-002-cloud-run-job.md) |
| TS-007 | Regression, runtime contract check, and delivery report | all above | [Verification](verification/05.1-feature-verification.md) |

Implementation proceeds by dependency readiness, not merely phase number.
