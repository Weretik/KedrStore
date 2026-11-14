# Background tasks «Фонові задачі»

**Static code check:** 2026-11-14. **Module:** Scheduling. **Spec:** [platform/007](../../specs/platform/007-background-tasks/README.md). **Contract:** [scheduling.openapi.yaml](../../contracts/scheduling/scheduling.openapi.yaml).

One Cloud Scheduler entry calls the tick every minute; the tick sends the outbox and starts every due task in one execution of the Cloud Run Job `kedr-tasks`. The list of tasks is code; an Admin changes only the schedule, the on/off switch and the next run, sees the last success and the last failure with its reason, and presses «Запустити зараз». Stocks (07:00 and 13:00) and prices (Monday 06:00) are separate tasks.

## Structure and relationships

[RunSchedulerTickEndpoint](../../../../src/Modules/Scheduling/Scheduling.Api/Endpoints/Tick/RunSchedulerTick/RunSchedulerTickEndpoint.cs) (`POST /internal/scheduler/tick`, anonymous for the auth middleware, accepted only with the scheduler's Google OIDC token by [SchedulerCallerCheck](../../../../src/Modules/Scheduling/Scheduling.Api/Endpoints/Tick/RunSchedulerTick/SchedulerCallerCheck.cs)) sends [RunSchedulerTickCommand](../../../../src/Modules/Scheduling/Scheduling.Application/Features/Tick/Commands/RunSchedulerTick/RunSchedulerTickCommand.cs). Its handler loads a row for every task of [ScheduledTaskCatalog](../../../../src/Modules/Scheduling/Scheduling.Application/Features/Catalog/ScheduledTaskCatalog.cs), runs the outbox ([OutboxTickWork](../../../../src/Modules/Scheduling/Scheduling.Infrastructure/Integrations/Outbox/OutboxTickWork.cs)), fails stuck runs, claims due tasks with the `Version` concurrency token ([ScheduledTask](../../../../src/Modules/Scheduling/Scheduling.Domain/Tasks/ScheduledTask.cs)) and starts the job through the Cloud Run Admin API ([CloudRunTaskJobLauncher](../../../../src/Modules/Scheduling/Scheduling.Infrastructure/Integrations/CloudRun/CloudRunTaskJobLauncher.cs), arguments `--job=tasks --tasks=key:runId,...`).

`Host.Jobs --job=tasks` runs [ScheduledTasksJobRunner](../../../../src/Bootstrapper/Host.Jobs/Tasks/ScheduledTasksJobRunner.cs): for each claimed task it marks the run started through [IScheduledTaskRunJournal](../../../../src/Modules/Scheduling/Scheduling.Contracts/Runs/IScheduledTaskRunJournal.cs), runs the [runner](../../../../src/Bootstrapper/Host.Jobs/Tasks/ScheduledTaskRunners.cs) of its key (the existing Catalog, Sales and Media jobs) and records the summary or the reason.

The admin endpoints `/api/admin/scheduling/tasks` (tag `AdminScheduledTasks`, `RequireAdminRole`) list tasks, read the last 50 runs, change a schedule ([UpdateScheduledTaskScheduleHandler](../../../../src/Modules/Scheduling/Scheduling.Application/Features/Tasks/Commands/UpdateScheduledTaskSchedule/UpdateScheduledTaskScheduleHandler.cs)) and request a run now. Schedules follow Kyiv wall-clock time ([TaskSchedule](../../../../src/Modules/Scheduling/Scheduling.Domain/Schedules/TaskSchedule.cs)).

## Code structure

```text
src/Modules/Scheduling/
├── Scheduling.Api/Endpoints/Tasks/               # admin list, runs, schedule, run now
├── Scheduling.Api/Endpoints/Tick/RunSchedulerTick/  # tick endpoint, options, OIDC caller check
├── Scheduling.Application/Features/Catalog/      # the code list of tasks and their keys
├── Scheduling.Application/Features/Tick/Commands/RunSchedulerTick/
├── Scheduling.Application/Features/Runs/         # run journal for the job
├── Scheduling.Application/Features/Tasks/        # admin commands, queries, schedule input, rows
├── Scheduling.Contracts/Runs/                    # IScheduledTaskRunJournal
├── Scheduling.Domain/{Schedules,Tasks,Runs}/
└── Scheduling.Infrastructure/{Persistence,Migrations,Integrations/CloudRun,Integrations/Outbox,DependencyInjection}/
src/Bootstrapper/Host.Jobs/Tasks/                 # --job=tasks; runners in Catalog/, Sales/, Site/
```

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Scheduling"` — schedules and Kyiv time, task states, tick, admin use cases, validators, `--tasks` parsing.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Scheduling"` — Admin-only access, schedule change, run now, the protected tick (SQLite, no Docker).

## Operations

Cloud switch-over and daily checks: [background tasks runbook](../../operations/jobs/background-tasks.md). Data: [scheduling tables](../../architecture/data-model/scheduling-tasks.md).
