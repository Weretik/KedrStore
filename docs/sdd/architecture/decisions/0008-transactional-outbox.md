# ADR-0008: Side effects leave through a transactional outbox per module

- Status: accepted
- Date: 2026-11-15 (recorded from the current code; [platform spec 006](../../specs/platform/006-transactional-outbox/README.md))
- Owner: Виталий (project owner)

## Context

Some use cases change data and must also tell the outside world: a quick order from the site goes to the managers' Telegram chat, an order goes to 1C. If the message is sent before the commit, a failed commit leaves a message about data that does not exist; if it is sent after, a crash between the two loses it. The API runs on Cloud Run, which scales to zero, so an in-process retry loop can stop at any time.

## Alternatives considered

- Send inside the request and hope: rejected; failures are lost.
- A message broker (Pub/Sub, RabbitMQ) with MassTransit: rejected; more infrastructure than a few messages a day need.
- Hangfire or a hosted background service: rejected; Cloud Run stops idle instances.

## Decision

- A use case writes an outbox row in the same `SaveChanges` as its data, in its own module's schema (`<schema>."OutboxMessages"`).
- After the commit the request tries the row once; what is left is retried by the dispatcher with growing delays (1, 5, 15, 30, 60 minutes, then hourly; 10 attempts).
- The outbox table, writer and dispatcher are shared code in `BuildingBlocks.Infrastructure`; each module registers its own handlers (`AddOutbox<TContext>`, `AddOutboxHandler<TMessage, THandler>`). Handlers are idempotent.
- The dispatcher runs inside the scheduler tick ([ADR-0009](0009-scheduler-tick-for-background-tasks.md)), not in a loop of its own.

## Why

- Data and its message are committed together or not at all.
- No extra infrastructure; the database already exists.

## Trade-offs

- A message may arrive late (up to the next tick) and more than once, so receivers must ignore repeats.
- Each module's outbox table must be watched for rows that ran out of attempts.

## Sources and checks

- `src/BuildingBlocks/BuildingBlocks.Infrastructure/Outbox/`, `src/Modules/Sales/Sales.Infrastructure/DependencyInjection/SalesInfrastructure.cs`.
- [Module rules: transactional outbox](../../standards/module-rules.md#transactional-outbox), [quick order](../../features/sales/quick-order.md).
