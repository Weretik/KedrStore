# Background tasks runbook

Spec [platform/007](../../specs/platform/007-background-tasks/README.md); feature [«Фонові задачі»](../../features/platform/background-tasks.md).

## How it runs

1. Cloud Scheduler calls `POST https://<api>/internal/scheduler/tick` every minute with a Google OIDC token.
2. The API sends the outbox, fails stuck runs, claims due tasks and starts one execution of the Cloud Run Job `kedr-tasks` with `--job=tasks --tasks=key:runId,...`.
3. The job runs the tasks one after another and writes each result; the admin page «Адміністрування → Фонові задачі» shows them.

The deploy workflow creates or updates `kedr-tasks` (4 h limit, no retries, secrets for 1C, Telegram and the media FTP), lets the API's runtime account start it, and sets `Scheduler__Job__Project`, `Scheduler__Job__Region`, `Scheduler__Job__Name` on the API.

## Switch-over (once, owner through the coordinator)

Until these steps are done the tick answers `404` and nothing changes: the old Cloud Run jobs keep their own Cloud Scheduler entries.

1. **EN-01.** The deploy workflow gives the API's runtime service account the role `roles/run.jobsExecutorWithOverrides` on the job `kedr-tasks` only, so this step needs no one by hand. The deploy account has Cloud Run Admin; the owner's day-to-day account cannot grant it.
2. **EN-02.** Create a service account for the scheduler (for example `kedr-scheduler`) and set on the API `Scheduler__Audience=https://<api run.app URL>/internal/scheduler/tick` and `Scheduler__SchedulerServiceAccount=<that account's e-mail>` (`gcloud run services update api --update-env-vars …`).
3. **EN-03.** Create one Cloud Scheduler job `kedr-tick`: every minute (`* * * * *`, time zone Europe/Kyiv), `POST` to that URL, OIDC token of that account with that audience. Check on the page that «Досилання повідомлень» shows a recent success.
4. The same day: on the page enable «Залишки з 1С», «Ціни з 1С», «Каталог з 1С», «Клієнти з 1С», «Замовлення в 1С», «Фото з CDN» (adjust the times if needed) and **pause** the old Cloud Scheduler entries of `onec-*` and `media-cdn-sync`.
5. A week later, when the page shows regular successes, delete the paused Cloud Scheduler entries and the old jobs `onec-stocks`, `onec-prices`, `onec-category`, `onec-productdetails`, `onec-pricetypes`, `onec-full`, `onec-orders-sync`, `onec-sales-customers-full`, `media-cdn-sync`, and remove their update lines from the deploy workflow.

Rollback: pause `kedr-tick` and resume the old entries; the tasks stay in the database and are ignored.

## Daily checks

- A task marked «Остання спроба з помилкою» shows the reason on the page; the full log is in Cloud Logging of the `kedr-tasks` execution at that time.
- «Задача не завершилася за N хв.»: the job was killed or hung; «Задача не почалася…»: the execution never reached it (earlier tasks took 4 hours, or the execution failed to start).
- «Не вдалося запустити задачу: …»: the API could not start `kedr-tasks` (EN-01 role, job name, region).
