# Work: tasks and activity

**Checked:** 2026-11-06 against `WorkDbContext` and its configurations. Schema `work`; migration history in `work."__EFMigrationsHistory"`.

```mermaid
erDiagram
    WorkTasks {
        bigint Id PK "identity"
        uuid OwnerUserId "Identity user ID, no FK"
        uuid CreatedByUserId "Identity user ID, no FK; differs from owner for an admin's task"
        varchar200 Title
        varchar4000 Details "null"
        varchar20 Status "Planned, InProgress, Done"
        varchar2000 Result "null; only while Done"
        timestamptz CreatedAt
        timestamptz UpdatedAt
        timestamptz StartedAt "null"
        timestamptz CompletedAt "null"
    }
    WorkActivities {
        bigint Id PK "identity"
        uuid UserId "Identity user ID, no FK"
        varchar256 UserEmail "sign-in email at the time"
        varchar64 Area "controller without Admin, e.g. Categories"
        varchar100 Operation "action method"
        varchar10 Method "POST, PUT, PATCH, DELETE"
        varchar300 Target "route values, e.g. id=12, lang=uk"
        int Count "repeats within 10 minutes"
        timestamptz FirstAt
        timestamptz OccurredAt "last repeat"
    }
```

Indexes: `WorkTasks(OwnerUserId, Status)` for the board, `WorkTasks(OwnerUserId, CompletedAt)` for the report, `WorkActivities(UserId, OccurredAt)` for the report and the merge of repeats. User IDs reference Identity's `AspNetUsers.Id` as values only, by the [module rules](../../standards/module-rules.md).
