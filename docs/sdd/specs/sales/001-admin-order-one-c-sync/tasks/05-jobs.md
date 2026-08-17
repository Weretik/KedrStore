# Phase 05 — Background delivery job

- [ ] [T145 — Dead-letter Telegram notification and Excel export](jobs/05.0-dead-letter-telegram.md)
- [ ] [T150 — `SyncOneCOrdersJob`: send and check 1C requests](jobs/05.1-sync-one-c-orders-job.md)
- [ ] [T160 — Cloud Run Job deployment and scheduling](jobs/05.2-cloud-run-order-sync.md)

## Checkpoint

`Host.Jobs` can execute `--job=sync-one-c-orders`; each run safely sends due orders, evaluates the synchronous 1C response, records the delivery state, alerts Telegram once when a record becomes dead-letter, and is deployed as a dedicated Cloud Run Job.
