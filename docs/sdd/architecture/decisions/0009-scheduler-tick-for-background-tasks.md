# ADR-0009: One scheduler tick runs the outbox and every due background task

- Status: accepted
- Date: 2026-11-14 (tick every five minutes chosen by the owner on 2026-11-14)
- Owner: Виталий (project owner)
- Supersedes: the "one Cloud Scheduler entry per job" part of [ADR-0002](0002-one-shot-jobs-on-cloud-run.md)

## Context

[ADR-0002](0002-one-shot-jobs-on-cloud-run.md) gave each job its own Cloud Scheduler entry. Schedules lived only in Google Cloud: the owner could not see or change them, did not know whether the last run succeeded, and a new task needed manual cloud work. The outbox also needed a regular trigger.

## Alternatives considered

- Keep one Cloud Scheduler entry per job: rejected; schedules and results invisible in the admin.
- An in-process scheduler (Hangfire, Quartz, hosted timer): rejected; Cloud Run scales to zero.
- A tick every minute: rejected by the owner; five minutes is precise enough and cheaper.

## Decision

- The Scheduling module owns the list of tasks (code), their schedules in Kyiv time, state and run history.
- One Cloud Scheduler entry calls `POST /internal/scheduler/tick` every five minutes with a Google OIDC token, checked in the endpoint.
- The tick sends the outbox, fails stuck runs, claims due tasks with the `Version` concurrency token and starts one execution of the Cloud Run Job `kedr-tasks` (`Host.Jobs --job=tasks --tasks=key:runId,...`).
- The job records the start, the summary or the failure reason for each task through `Scheduling.Contracts`.
- In the admin «Фонові задачі» an Admin changes only the schedule, the on/off switch and the next run, and can press «Запустити зараз».

## Why

- The owner sees and controls every periodic task without opening Google Cloud.
- A new task is code plus a catalog entry; no cloud change.
- Two ticks cannot start the same task twice.

## Trade-offs

- A task starts up to five minutes after its time.
- The tick endpoint is public on Cloud Run and relies on the OIDC check.

## Sources and checks

- [Background tasks](../../features/platform/background-tasks.md), [runbook](../../operations/jobs/background-tasks.md).
- Unit and integration tests under `UnitTests.Scheduling` and `IntegrationTests.Scheduling`.
