# Quality targets

Measurable goals for the Kedr production system. They tell when the system is "slow" or "down" before a user does, they are the thresholds the alerts use, and **every architecture review, audit and feature design starts from them**: a speed, reliability or scaling verdict without these numbers is not given.

Status of each row:

- **confirmed**: the owner confirmed the number on 2026-11-17; change it only together with its alert.
- **proposed**: added on 2026-11-19 to close the gaps of the first version; the number is a starting point to confirm with the owner after the first measurement.

The **Current** column holds the last measured value with its date. "Not measured" is an open gap, not a pass; the [measurement plan](#measurement-plan) says how to fill it.

## Scale the targets are set for

| What | Value | Source |
| --- | --- | --- |
| Users | Staff in the admin, managers in the mobile app «Кедр Менеджер», wholesale and retail customers on the public site | Product description |
| Products in the catalogue | About 2 330 (catalogue export of 2026) | [`products_attributes.csv`](../../../catalog-attributes/products_attributes.csv); confirm with the row count below |
| API requests | About 8 100 a day; peak 925 a minute (2026-11-09 06:04 UTC) and 5 349 an hour | Cloud Run request logs, 2026-10-25 → 2026-11-19 (measured 2026-11-19) |
| Orders per day, site visitors per day | Not measured | Measurement plan, steps 4 and 6 |
| Hosting | API `api` on Cloud Run in `europe-central2`, 0 to 20 instances (no minimum instance, so the first request after a quiet period waits for a cold start), 1 vCPU and 512 MiB each, 80 requests per instance; PostgreSQL managed on adm.tools (outside Google Cloud, so every query crosses the network between the two); public site on the Cloudflare Worker `kedr-site`; API behind the Worker `api.classkedr.com.ua` | [Deploy workflow](../../../../.github/workflows/deploy-cloudrun.yml), [backups](../../operations/data/backups-and-restore.md) |
| Design horizon (proposed) | ×10 the current catalogue and order volume without a change of architecture | Checked in reviews against the measured volume |

## Speed

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Reads, P95 | 95% of `GET` requests answer within 1 second | confirmed | **Met:** 328 ms (P50 50 ms, 195 569 requests, 2026-10-25 → 2026-11-19) | Cloud Run request latency of `api` (traces as `kedr-api`) |
| Reads, P99 | 99% of `GET` requests answer within 3 seconds | proposed | **Met:** 882 ms (same window) | Same |
| Writes, P95 | 95% of other requests answer within 2 seconds | confirmed | **Met:** 1 159 ms (1 913 requests, same window) | Same |
| Writes, P99 | 99% of other requests answer within 5 seconds | proposed | **Met:** 1 993 ms (same window) | Same |
| Public site pages | At the 75th percentile of mobile visits: LCP ≤ 2.5 s, INP ≤ 200 ms, CLS ≤ 0.1 (Google's "good" Core Web Vitals) | proposed | Not measured | Search Console «Core Web Vitals»; PageSpeed Insights for the home, a category and a product page while field data is too small |
| Peak capacity | The API holds three times the measured peak requests per minute with reads still under their P95 target | proposed | Measured peak 925 requests a minute (2026-11-09), 5 349 an hour; about 8 100 requests a day. The ×3 load was never tested | Measurement plan, step 2; a load test only before a known peak |

A cold start of a new Cloud Run instance falls into P99. Measured on 2026-11-19: the first request after more than 15 quiet minutes took 3.8 s at the median and 5.2 s at P95 (366 cases in 24 days); the slowest public pages (category pages, `/api/auth/session/me`) are slow only at this tail. If P99 misses because of cold starts, the decision (a minimum instance, which costs money every month) is made with the cost row below, not by changing the target.

## Availability and errors

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Availability of the API | 99.5% of each month (about 3.5 hours of downtime a month) | confirmed | Not measured: the only uptime check (`kedr-api-health`) calls `/health` every 10 minutes, too rarely to count minutes of downtime | Uptime check on the API's `/health/ready`; share of minutes without 5xx bursts in the Cloud Run request logs |
| Server errors | Under 1% of requests answer 5xx in any 15 minutes | confirmed | **Missed once:** 0.08% of all requests answered 5xx, but on 2026-11-14 19:15 one 15-minute window had 35% (31 of 89). No alert on the 5xx share | Cloud Run request logs, alert «API 5xx» |
| Background tasks | A scheduled task that fails or does not run for twice its period is noticed within 1 hour | confirmed | No «Job failed» alert exists | «Фонові задачі» page and job error logs, alert «Job failed» |
| Manager app | At least 99% of sessions without a crash | proposed | Not measured | Google Play Console → Android vitals |

## Data freshness

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Orders reach 1C | An order is sent to 1C within 15 minutes of being placed | confirmed | Not measured | Sales outbox «Відправка замовлень в 1С» and its DeadLetter Telegram alert |
| Stock from 1C | Never older than the last scheduled run of «Залишки з 1С» (07:00 and 13:00 Kyiv, so at most about 18 hours overnight) | proposed | Task enabled: to check | Last success of the task on «Фонові задачі» |
| Prices from 1C | Never older than 7 days (Monday 06:00 run); after a price change in 1C a manager runs «Ціни з 1С» by hand | proposed | Task enabled: to check | Same |
| Catalogue, customers and photos from 1C and the CDN | Never older than 24 hours (nightly runs at 03:00, 04:00, 05:00) | proposed | Tasks enabled: to check | Same |

The catalogue tasks are off by default in the code; a target counts only once its task is enabled on «Фонові задачі».

## Data safety

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Data loss (RPO) | At most 24 hours of data, the age of the latest hosting backup | confirmed | Met by design: a backup every day, kept 2 months (owner, 2026-11-19); the age of the latest backup is not alerted | [Backups and restore](../../operations/data/backups-and-restore.md) |
| Recovery time (RTO) | Service back within 4 hours after the database is lost | confirmed | Never timed: no restore drill yet | Restore procedure in the same document |

## Cost

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Monthly running cost | A budget the owner sets for Google Cloud, Cloudflare, adm.tools, the CDN and Google Play together; a Google Cloud budget alert at 50%, 90% and 100% | proposed, number to set | Not measured | Google Cloud Billing reports and the other providers' invoices |

## Gaps in measurement

Baseline measured on 2026-11-19 from 24 days of Cloud Run request logs (the newest 200 000 rows, 2026-10-25 → 2026-11-19). Each gap is open until the target has a current value and an alert that is known to exist.

1. **Alerts cover only "down".** The only alert policy is «Kedr API down» on the uptime check `kedr-api-health`, which calls `/health` (liveness, not `/health/ready`, so a lost database is not noticed) every 10 minutes. There is no alert for the 5xx share, P95, failed jobs, or the backup age, though this document names them.
2. **A 5xx burst went unnoticed:** 35% of requests failed in one 15-minute window on 2026-11-14 19:15 UTC. Its cause is not recorded.
3. **No availability figure,** because the uptime check runs too rarely; no dashboard shows P95, P99 and the error share against these targets.
4. **Restore never tried.** Backups run daily and are kept 2 months (owner, 2026-11-19), but a restore was never timed, so RTO is an assumption.
5. **No budget alert** and no monthly cost figure.
6. **Core Web Vitals and app crashes** of the public site and the manager app were never checked; freshness tasks were not checked as enabled.
7. **`claude-readonly` cannot read Cloud Run settings** (`run.services.get` denied); the settings above come from the deploy audit log. Give it `roles/run.viewer` to read them directly.

## Measurement plan

Read-only steps; none changes the system. The owner runs the console steps; an agent can run the `gcloud` steps through Remote Control on the owner's computer with impersonation (`--impersonate-service-account=claude-readonly@<project>.iam.gserviceaccount.com`).

1. **Read-only access.** `claude-readonly` has `roles/monitoring.viewer` since 2026-11-19; add `roles/run.viewer` to read the Cloud Run settings too. With these, latency, request count and alert policies can be read without logs: `gcloud alpha monitoring policies list`, and the metrics `run.googleapis.com/request_latencies` and `run.googleapis.com/request_count` of service `api`.
2. **Latency and peak from request logs** (Logs Viewer is enough): read 30 days of `resource.type="cloud_run_revision" AND resource.labels.service_name="api"` with `--format="csv(timestamp,httpRequest.requestMethod,httpRequest.latency,httpRequest.status)"`, then compute P95 and P99 for `GET` and for other methods, the 5xx share per 15 minutes, and the busiest minute. Write the four numbers and the date into **Current**. A busy month has more than 200 000 log rows: export one day at a time, or the window is cut short.
3. **Alerts.** In Cloud Monitoring → Alerting, list the policies. Create the missing ones from the thresholds above: uptime check on `/health/ready`, 5xx share over 1% for 15 minutes, P95 of `GET` over 1 s for 15 minutes, failed job. Name them as this document does and send them to the owner.
4. **Data volume.** In the adm.tools database console, read-only: `SELECT pg_size_pretty(pg_database_size(current_database()));`, `SELECT schemaname, relname, n_live_tup FROM pg_stat_user_tables ORDER BY n_live_tup DESC LIMIT 15;`, and the number of rows in `sales."Orders"` created per day over the last 90 days.
5. **Backups.** Frequency and retention are recorded (daily, 2 months). Once a quarter restore the latest backup into a new database and time it: that time is the current RTO.
6. **Site and app.** Search Console → «Core Web Vitals» for mobile and desktop; Google Play Console → Android vitals for crash-free sessions.
7. **Cost.** Google Cloud Billing → Reports for the last three months, plus the other providers' invoices; the owner sets the budget and creates the budget alert.

## How they are used

- Every architecture review, code audit and new-feature design starts by reading this file. Missing targets or missing current values are the review's first finding; a feature on a hot path (catalogue, orders, site pages, 1C sync) states its expected load and checks it does not move P95 or P99 past the target.
- Alerts in Cloud Monitoring use these thresholds and notify the owner.
- Once a month compare the last 30 days with the table and update **Current**; a target missed twice in a row becomes a task, not a new number.
- A proposed row becomes confirmed when the owner accepts its number; then it gets its alert.
