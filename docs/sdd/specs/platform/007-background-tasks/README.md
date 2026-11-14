# platform/007 — «Фонові задачі»: one scheduler, schedules in the database

**Status:** implemented 2026-11-14 (cloud switch-over EN-01..EN-03 pending). **Module:** Scheduling (new, schema `scheduling`); runners in `Host.Jobs`; tick in `Host.Api`. **Owner decision:** 2026-11-14, thread «Управлять фоновыми задачами из админки» (plan accepted with «да»; stocks and prices are separate tasks: stocks twice a day, prices weekly).

## Why

Kedr has about fifteen Cloud Run Jobs (`onec-stocks`, `onec-prices`, `onec-full`, `onec-orders-sync`, `onec-sales-customers-full`, `media-cdn-sync`, …), each with its own Cloud Scheduler entry and arguments that live only in the cloud console. Their results are visible only in Cloud Logging, only the owner can change a time, and the outbox of platform/006 has no scheduled dispatch yet.

## Decision

One Cloud Scheduler job calls `POST /internal/scheduler/tick` every minute. The tick runs the outbox, then starts every due task in one execution of the Cloud Run Job `kedr-tasks` (`Host.Jobs --job=tasks --tasks=<key:runId,...>`). The list of tasks is code; schedules, the on/off switch and run results are rows of the new module Scheduling. The admin page «Адміністрування → Фонові задачі» (kedr-web) shows them, changes the schedule and starts a task now.

Why a new module: no module owns schedules or run history; Catalog, Sales and Media own the work and stay unchanged. Alternatives rejected: a Cloud Scheduler job per task (paid past three jobs, arguments outside the repository); starting the job every minute (1440 executions a day); running 1C imports inside the API request (minutes long).

Not in scope: `migrate` (deploy), catalogue attribute imports, `one-c-smoke-write`, `migrate-slugs` and the targeted `--rootId` jobs: they stay `Host.Jobs --job=<key>` commands for a developer.

## Rules

- **R-01 Tasks are code.** `ScheduledTaskCatalog` lists every task: key, Ukrainian name and description, default schedule, longest allowed run, editable or not, inside the tick or in the job. The admin cannot add, rename or delete a task.
- **R-02 Schedule.** `Interval` (every N minutes, 5–1440, optionally between two Kyiv hours), `Daily` (1–6 Kyiv times on chosen weekdays) or `Manual`. Saving recomputes the next run; the admin may move the next run once to a later instant.
- **R-03 On/off.** A disabled task is never started by the schedule; «Запустити зараз» still runs it.
- **R-04 Tick.** (a) outbox (`IOutboxDispatcher.DispatchDueAsync`), result on task `outbox`; (b) a run longer than its limit since it started, or not started within 240 minutes, is failed; (c, d) due tasks are claimed with the `Version` concurrency token, so two ticks never start one task; (e) one job execution with all claimed tasks; (f) runs older than 90 days are deleted. A failure of one part does not stop the others.
- **R-05 Job start fails.** Every claimed task is failed with the reason and gets its next run.
- **R-06 Run.** `Host.Jobs --job=tasks --tasks=key:runId,...` runs the tasks one after another, each in its own scope, through `IScheduledTaskRunJournal` (Scheduling.Contracts): start, run, success with a short summary or failure with the main reason (≤ 500 characters). A stale run is skipped; the exit code is 1 when any task failed.
- **R-07 Next run** is the first schedule instant after the finish; empty for `Manual`.
- **R-08 Run now** records who pressed it; the next tick starts it; 409 while running or already requested.
- **R-09 What the admin sees:** name, description, schedule, next run, running since, last success and summary, last failure and reason, on/off; marks only for problems (last result failed, missed by more than 15 minutes); the last 50 runs.
- **R-10 Protected tick.** Only a Google OIDC token with audience `Scheduler:Audience` from `Scheduler:SchedulerServiceAccount` passes (401 otherwise); without that configuration the tick answers 404.
- **R-11 Access.** `/api/admin/scheduling/tasks` needs `RequireAdminRole`.
- **R-12 Switch-over.** Tasks start **disabled** (except the outbox and the manual ones), so the old Cloud Scheduler entries and the new schedule never both run them. The owner enables them on the page the day the old entries are paused.

## Tasks

| Key | Name | Work (Host.Jobs runner) | Default |
| --- | --- | --- | --- |
| `outbox` | Досилання повідомлень | outbox of every module, inside the tick | every minute, not editable |
| `stocks` | Залишки з 1С | `SyncOneCStocksJob` for the three roots, one list rebuild | every day 07:00 and 13:00 |
| `prices` | Ціни з 1С | `SyncOneCPriceTypesJob`, `SyncOneCPricesJob` for the three roots, one list rebuild | Monday 06:00 |
| `catalog` | Каталог з 1С | categories, product details, attribute recognition, list rebuild | every day 03:00 |
| `customers` | Клієнти з 1С | `SyncOneCSalesCustomersFullJob` | every day 04:00 |
| `orders` | Замовлення в 1С | `SyncOneCOrdersJob` (retries and dead letters) | every 10 minutes |
| `media` | Фото з CDN | `SyncCdnMediaJob`, `LinkProductMediaJob`, `ArrangeProductMediaFoldersJob` | every day 05:00 |
| `full-sync` | Повне оновлення з 1С | `SyncOneCFullJob` | manual |
| `product-list` | Перебудувати список товарів | `IProductListProjectionRebuilder` | manual |

The three roots are `OneC:DoorsRootCategoryId`, `OneC:HardwareRootCategoryId`, `OneC:CosmosRootCategoryId` of `Host.Jobs/appsettings.json`.

## Scenarios

- **SC-01** A due idle task → one job execution with its key; a second tick starts nothing.
- **SC-02** Two tasks due at once → one execution, a run row each.
- **SC-03** Success → summary, idle, next run. **SC-04** Failure → reason, last success kept, next run; the next task still runs.
- **SC-05** Killed job → failed after its limit. **SC-06** Job cannot start → claimed tasks failed with the reason.
- **SC-07** Admin sets stocks to 07:00 and 13:00 → next run is the next of those Kyiv times; invalid schedule → 400; the outbox → 409.
- **SC-08** «Запустити зараз» on a manual task → started by the next tick; again while it runs → 409.
- **SC-09** Not Admin → 401/403 on every endpoint.
- **SC-10** Tick without the scheduler's token → 401; not configured → 404; the scheduler's token → 200 with the started keys.
- **SC-11** Kyiv daylight-saving changes keep the wall-clock times.

## HTTP

| Method, route | operationId | Body / answer |
| --- | --- | --- |
| GET `/api/admin/scheduling/tasks` | `getAdminScheduledTasks` | tasks in catalogue order |
| GET `/api/admin/scheduling/tasks/{key}/runs` | `getAdminScheduledTaskRuns` | last 50 runs; 404 unknown key |
| PUT `/api/admin/scheduling/tasks/{key}/schedule` | `updateAdminScheduledTaskSchedule` | `{ enabled, schedule: { kind, everyMinutes?, fromHour?, toHour?, times?, days? }, nextRunAt? }`; 400, 404, 409 not editable |
| POST `/api/admin/scheduling/tasks/{key}/run` | `runAdminScheduledTaskNow` | task; 404, 409 |
| POST `/internal/scheduler/tick` | — (not in the contract) | `{ inline, started, launchError }` |

Contract: [scheduling.openapi.yaml](../../../contracts/scheduling/scheduling.openapi.yaml).

## Data

Schema `scheduling`: `ScheduledTasks` (key, on/off, schedule jsonb, next run, state, current run, run requested by/at, last success/failure, `Version` concurrency token) and `ScheduledTaskRuns` (Guid v7 id created by the tick, task key, trigger, claimed/started/finished, outcome, message). No foreign keys to other modules. Details: [data model](../../../architecture/data-model/scheduling-tasks.md). Rollback: revert the code; the schema stays and is ignored.

## Design

| Layer | Path (under `src/Modules/Scheduling/`) | Responsibility |
| --- | --- | --- |
| Domain | `Scheduling.Domain/{Schedules,Tasks,Runs}/` | schedule value object and Kyiv time, task claim/start/finish/expire, run |
| Application | `Features/Catalog/` | the code list of tasks and keys (R-01) |
| Application | `Features/Tick/Commands/RunSchedulerTick/` | R-04, R-05 |
| Application | `Features/Runs/` | `ScheduledTaskRunJournal` (R-06, R-07) |
| Application | `Features/Tasks/{Commands,Queries,Shared}/` | admin use cases, schedule input and validator |
| Contracts | `Scheduling.Contracts/Runs/IScheduledTaskRunJournal.cs` | what the job host records |
| Infrastructure | `Persistence/` (`Tasks/` configurations), `Migrations/`, `Integrations/CloudRun/`, `Integrations/Outbox/` | schema, Cloud Run `jobs:run`, outbox in the tick |
| Api | `Endpoints/Tasks/`, `Endpoints/Tick/RunSchedulerTick/` | HTTP, OIDC caller check |
| Host.Jobs | `Tasks/` (`ScheduledTasksJobRunner`, `ScheduledTaskRunners`, runners in `Catalog/`, `Sales/`, `Site/`) | R-06 |
| Deploy | `deploy-cloudrun.yml`: job `kedr-tasks` (4 h, no retries), API env `Scheduler__Job__*` | |

## Prerequisites (cloud, owner through the coordinator)

- **EN-01** The API's runtime service account: `roles/run.jobsExecutorWithOverrides` on job `kedr-tasks`.
- **EN-02** API settings `Scheduler__Audience` (API URL + `/internal/scheduler/tick`) and `Scheduler__SchedulerServiceAccount`; the deploy sets `Scheduler__Job__*`.
- **EN-03** One Cloud Scheduler job, every minute, `POST` the tick with an OIDC token of that account and that audience. Then enable the tasks on the page and pause the old Cloud Scheduler entries the same day; delete them and the old jobs a week later.

Runbook: [background tasks](../../../operations/jobs/background-tasks.md).

## Tasks and evidence

- **TS-01** Domain tests (schedules, DST, claim, expiry). **TS-02** Tick tests (SC-01, SC-02, SC-05, SC-06, outbox). **TS-03** Journal and `--tasks` parsing, a runner for every job task. **TS-04** Admin use cases and validators (SC-07, SC-08); HTTP access and tick (SC-09, SC-10). **TS-05** Migration `InitialScheduling`. **TS-06** Deploy job, docs.
- **Evidence 2026-11-14:** `dotnet test tests/UnitTests --filter UnitTests.Scheduling` 55 passed; `tests/IntegrationTests --filter IntegrationTests.Scheduling` 7 passed (SQLite, no Docker); `tests/ArchitectureTests` 48 passed; OpenAPI snapshot updated. Not written: a PostgreSQL test of the concurrent claim.
- **Final audit 2026-11-14:** handler and job-runner tests moved from `UnitTests` (which may not reference a host or Infrastructure) to `IntegrationTests/Scheduling/{Api,Tasks}` and `IntegrationTests/Platform/Jobs`; persistence configurations moved to `Persistence/Tasks/`. Unit filter `Scheduling` 38 passed; integration filter `Scheduling|ScheduledTasksJobRunner|OpenApiSnapshot` 30 passed; `tests/ArchitectureTests` 48 passed.
