# Quality targets

Measurable goals for the Kedr production system. They tell when the system is "slow" or "down" before a user does, and they are the thresholds the alerts use. Status: **confirmed by the owner on 2026-11-17**; change a number only together with its alert.

Scale they are set for: a small team of staff users plus the public site, one Cloud Run API instance most of the time, one PostgreSQL database.

| Target | Value | Measured by |
| --- | --- | --- |
| Availability of the API | 99.5% of each month (about 3.5 hours of downtime a month) | Cloud Run request logs of `api` (Cloud Run service; traces as `kedr-api`): share of minutes without 5xx bursts |
| Server errors | Under 1% of requests answer 5xx in any 15 minutes | Cloud Run request logs, alert «API 5xx» |
| Read speed (P95) | 95% of `GET` requests answer within 1 second | Cloud Trace latency of `api` (Cloud Run service; traces as `kedr-api`) |
| Write speed (P95) | 95% of other requests answer within 2 seconds | Cloud Trace latency |
| Background tasks | A scheduled task that fails or does not run for twice its period is noticed within 1 hour | «Фонові задачі» page and job error logs, alert «Job failed» |
| Orders reach 1C | An order is sent to 1C within 15 minutes of being placed | Outbox «Відправка замовлень в 1С» and its DeadLetter Telegram alert |
| Data loss (RPO) | At most 24 hours of data, the age of the latest hosting backup | [Backups and restore](../../operations/data/backups-and-restore.md) |
| Recovery time (RTO) | Service back within 4 hours after the database is lost | Restore procedure in the same document |

## How they are used

- Alerts in Cloud Monitoring use these thresholds (5xx share, P95 latency, failed job) and notify the owner.
- Once a month compare the last 30 days in Cloud Trace and Cloud Run metrics with the table; a target missed twice in a row becomes a task, not a new number.
- A feature that changes a hot path (catalogue, orders, site pages) checks it does not move P95 past the target.
