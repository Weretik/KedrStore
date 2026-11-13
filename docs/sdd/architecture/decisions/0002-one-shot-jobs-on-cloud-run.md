# ADR-0002: 1C sync and migrations as one-shot jobs on Cloud Run Jobs

- Status: accepted

## Context

1C is the source of truth for products, prices, stock and customers. Data must be imported regularly, and manager orders must be sent back to 1C. Running background loops inside the API on Cloud Run is unreliable: the service scales to zero and an instance can stop at any time. Applying database migrations at API startup is risky when several instances start at once.

## Alternatives considered

- Hosted background services inside the API: rejected; Cloud Run scales to zero and can stop an instance at any time.
- A message queue with workers: rejected for now; more infrastructure than the import volume needs.
- Migrations at API startup: rejected; several instances could migrate at once.

## Decision

- `Host.Jobs` is a console host that runs exactly one command and exits: `--job=migrate`, `--job=full`, `--job=prices`, `--job=stocks`, `--job=sales-customers-full`, `--job=sync-one-c-orders`, `--job=media-cdn-sync` and others.
- Each command is a Cloud Run Job. Cloud Scheduler starts each job on its own schedule.
- The deploy pipeline runs the migration job first and deploys the new API only if it succeeds. The API never migrates on startup.
- The order sync job runs one task with parallelism 1 and no platform retries; the job itself tracks attempts, keeps the 1C document number, and sends a Telegram alert when an order cannot be delivered.

## Why

- Jobs are cheap: they cost only while they run.
- A failed import is visible as a failed job execution and does not affect the API.
- Migrations happen once, before new code receives traffic.

## Trade-offs

- Schedules live in Google Cloud, not in the repository; a new environment needs them created by hand.
- There is no queue: if 1C is down, the next scheduled run picks up the work later.
- The same job image must stay compatible with the API image deployed next to it.
