# Work 001 — Staff tasks and the activity log

**Status:** accepted 2026-11-06 (owner chose variant «B + автосписок із C»); stages 1–2 implemented, stages 3–4 in kedr-web. **Module:** Work (new).

## Goal and boundaries

The owner asked (2026-11-06) that the new SEO specialist can note what they do: create tasks for themselves and mark them done, while the administrator sees their tasks and what they actually changed as a work report. Of the three proposed variants (A: daily log entries, B: own task board, C: automatic change log plus a daily comment) the owner chose **B with the automatic list from C** (no daily comment).

Stages, each with a structure check:

1. **Tasks (backend).** Module Work, schema `work`; own task board and the admin's task for a staff member.
2. **Activity log (backend).** Every successful change a staff member makes through an admin endpoint is written down automatically; the report reads tasks and activity for a period.
3. **«Мої задачі» (kedr-web).** Board «План / В роботі / Готово» with the right edit panel, and «Що я змінив» under it.
4. **Work report (kedr-web, Admin).** «Система → Звіт роботи»: a person and a period, finished tasks with their results, open tasks, and the changes by day.

Out of scope: time tracking, reminders, comments threads, deleting old activity (later, if the table grows), the mobile app.

## Module decision

A new module `Work` (Domain, Application, Infrastructure, Api) under the [module rules](../../../standards/module-rules.md): tasks and the activity log are data no existing module owns and serve every staff role, not only content or catalog work. Alternatives: Identity (rejected: it owns accounts and sign-in, not daily work) and Content (rejected: site texts). Tables live in schema `work` with their own migration history. Users are referred to by their Identity user ID (`Guid`) as a value, with no foreign key; names come from the admin users list on the client.

The activity log is written by a global MVC result filter in `Work.Api`, so no other module changes or knows about Work.

## Rules

- R-1: a task belongs to one staff member (`OwnerUserId`), has a title (1–200 characters, trimmed), optional details (up to 4000), a state `Planned`, `InProgress` or `Done`, and an optional result (up to 2000) written when it is done. It records who created it and when it was created, started, completed, and last changed.
- R-2: a staff member (Admin, Seo, Manager) sees, creates, edits, moves between states and deletes only their own tasks. Another person's task answers `404`.
- R-3: moving to `InProgress` sets the start time once; moving to `Done` sets the completion time and keeps the result; moving back from `Done` clears the completion time and the result.
- R-4: an administrator creates a task for any staff member (`POST /api/admin/work/tasks`); it appears on that person's board with «Від адміністратора».
- R-5: after a `POST`, `PUT`, `PATCH` or `DELETE` to a controller named `Admin*` (every admin endpoint) answers `2xx`, the log stores the user ID, the sign-in email, the time, the area (controller name without `Admin` and `Controller`, for example `Categories`), the operation (action method name), the HTTP method, and the route values (for example `id=12, lang=uk`). Work's own endpoints are not logged. A failure to log never fails the request.
- R-6: the same user repeating the same area, operation and route values within 10 minutes updates the last entry (time and count) instead of adding a row, so saving a text five times is one line.
- R-7: the report takes a user and a period of at most 92 days (`from`, `to` as dates in Kyiv time, inclusive) and returns the tasks finished in the period, the tasks still open, and the activity entries of the period, newest first, at most 2000. A staff member reads their own report; an administrator reads anyone's.
- R-8: (reserved: the admin «Для кого» choice is frontend only and uses R-4.)
- R-9: a task has up to 30 steps in order, each a text of 1–200 characters (trimmed) with a done mark. Create and edit send the whole list, which replaces the saved one; `PUT /api/work/tasks/{id}/steps/{index}` ticks or unticks one step of an own task (`404` for a position that does not exist). Steps are stored with their task.
- R-10: a task may have a due date (a calendar day, no time). It is overdue (`isOverdue`) while not `Done` and the due day has passed in Kyiv; a task due today is not overdue. The creation time (R-1) is when the task was set.

## Design

| Layer | Files |
| --- | --- |
| Domain | `Tasks/WorkTask.cs`, `WorkTaskStep.cs`, `WorkTaskId.cs`, `WorkTaskStatus.cs`; `Activity/WorkActivity.cs`, `WorkActivityId.cs`; `WorkErrors.cs` |
| Application | `Features/Tasks/{GetMine,Create,Update,ChangeStatus,SetStep,Delete,CreateForUser,Common}` (`WorkCalendar`: Kyiv day); `Features/Activity/{Record,Common}`; `Features/Report/GetReport`; `Contracts/Persistence` |
| Infrastructure | `DataBase/WorkDbContext.cs` (schema `work`), design-time factory, configurations, repository, migrations `InitialWork`, `WorkTaskStepsAndDueDate`, `DependencyInjection/WorkInfrastructureExtensions.cs` |
| Api | `Controllers/WorkTasksController.cs` (`/api/work/tasks`, `/api/work/report`, policy `RequireManagerRole`), `Controllers/AdminWorkController.cs` (`/api/admin/work`, `RequireAdminRole`), `Activity/WorkActivityFilter.cs` |
| Host | registration in `ModulesRegistrationExtensions`, Mediator and validators, controllers part and the global filter; Jobs host for the migration |

Contract: [work.openapi.yaml](../../../contracts/work/work.openapi.yaml).

## Tests

- Unit: `WorkTaskTests` (R-1, R-3, R-9, R-10), `WorkActivityTests` (R-6), validators.
- Integration: `WorkApiTests` (R-2, R-4, R-5, R-7, R-9, R-10) on the real API with PostgreSQL (CI).
