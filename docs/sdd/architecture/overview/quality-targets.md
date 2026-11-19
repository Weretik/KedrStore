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
| Orders per day, site visitors per day, peak requests per minute | Not measured | Measurement plan, steps 2 and 4 |
| Hosting | API `api` on Cloud Run in `europe-central2`, one instance most of the time; PostgreSQL managed on adm.tools (outside Google Cloud, so every query crosses the network between the two); public site on the Cloudflare Worker `kedr-site`; API behind the Worker `api.classkedr.com.ua` | [Deploy workflow](../../../../.github/workflows/deploy-cloudrun.yml), [backups](../../operations/data/backups-and-restore.md) |
| Design horizon (proposed) | ×10 the current catalogue and order volume without a change of architecture | Checked in reviews against the measured volume |

## Speed

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Reads, P95 | 95% of `GET` requests answer within 1 second | confirmed | Not measured | Cloud Run request latency of `api` (traces as `kedr-api`) |
| Reads, P99 | 99% of `GET` requests answer within 3 seconds | proposed | Not measured | Same |
| Writes, P95 | 95% of other requests answer within 2 seconds | confirmed | Not measured | Same |
| Writes, P99 | 99% of other requests answer within 5 seconds | proposed | Not measured | Same |
| Public site pages | At the 75th percentile of mobile visits: LCP ≤ 2.5 s, INP ≤ 200 ms, CLS ≤ 0.1 (Google's "good" Core Web Vitals) | proposed | Not measured | Search Console «Core Web Vitals»; PageSpeed Insights for the home, a category and a product page while field data is too small |
| Peak capacity | The API holds three times the measured peak requests per minute with reads still under their P95 target | proposed | Peak not measured | Measurement plan, step 2; a load test only before a known peak |

A cold start of a new Cloud Run instance falls into P99. If P99 misses because of cold starts, the decision (a minimum instance, which costs money every month) is made with the cost row below, not by changing the target.

## Availability and errors

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Availability of the API | 99.5% of each month (about 3.5 hours of downtime a month) | confirmed | Not measured | Uptime check on the API's `/health/ready`; share of minutes without 5xx bursts in the Cloud Run request logs |
| Server errors | Under 1% of requests answer 5xx in any 15 minutes | confirmed | Not measured | Cloud Run request logs, alert «API 5xx» |
| Background tasks | A scheduled task that fails or does not run for twice its period is noticed within 1 hour | confirmed | Not measured | «Фонові задачі» page and job error logs, alert «Job failed» |
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
| Data loss (RPO) | At most 24 hours of data, the age of the latest hosting backup | confirmed | Backup frequency and retention not yet read from the panel | [Backups and restore](../../operations/data/backups-and-restore.md) |
| Recovery time (RTO) | Service back within 4 hours after the database is lost | confirmed | Never timed: no restore drill yet | Restore procedure in the same document |

## Cost

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Monthly running cost | A budget the owner sets for Google Cloud, Cloudflare, adm.tools, the CDN and Google Play together; a Google Cloud budget alert at 50%, 90% and 100% | proposed, number to set | Not measured | Google Cloud Billing reports and the other providers' invoices |

## Gaps in measurement

Found on 2026-11-19. Each is open until the target has a current value and an alert that is known to exist.

1. **No baseline.** No row has a measured current value, so nobody can say today whether a target is met.
2. **Alerts are named, not verified.** «API 5xx», «Job failed», the P95 alert and the uptime check are named here and in [dependency failures](../../operations/diagnostics/dependency-failures.md), but no list of the real alert policies is kept, and the read-only account `claude-readonly` (Logs Viewer, Cloud Trace User) cannot list them or read Cloud Monitoring metrics.
3. **No dashboard** shows P95, P99 and the error share against these targets.
4. **Backups unverified.** The backup frequency and retention are not recorded and a restore was never timed, so RPO and RTO are assumptions.
5. **No budget alert** and no monthly cost figure.
6. **Core Web Vitals and app crashes** of the public site and the manager app were never checked against a target.

## Measurement plan

Read-only steps; none changes the system. The owner runs the console steps; an agent can run the `gcloud` steps through Remote Control on the owner's computer with impersonation (`--impersonate-service-account=claude-readonly@<project>.iam.gserviceaccount.com`).

1. **Give the read-only account metrics.** Add the role `roles/monitoring.viewer` to `claude-readonly`. Then latency, request count and alert policies can be read without logs: `gcloud alpha monitoring policies list`, and the metrics `run.googleapis.com/request_latencies` and `run.googleapis.com/request_count` of service `api`.
2. **Latency and peak from request logs** (works with the current role): read 30 days of `resource.type="cloud_run_revision" AND resource.labels.service_name="api"` with `--format="csv(timestamp,httpRequest.requestMethod,httpRequest.latency,httpRequest.status)"`, then compute P95 and P99 for `GET` and for other methods, the 5xx share per 15 minutes, and the busiest minute. Write the four numbers and the date into **Current**.
3. **Alerts.** In Cloud Monitoring → Alerting, list the policies. Create the missing ones from the thresholds above: uptime check on `/health/ready`, 5xx share over 1% for 15 minutes, P95 of `GET` over 1 s for 15 minutes, failed job. Name them as this document does and send them to the owner.
4. **Data volume.** In the adm.tools database console, read-only: `SELECT pg_size_pretty(pg_database_size(current_database()));`, `SELECT schemaname, relname, n_live_tup FROM pg_stat_user_tables ORDER BY n_live_tup DESC LIMIT 15;`, and the number of rows in `sales."Orders"` created per day over the last 90 days.
5. **Backups.** In the adm.tools panel, read how often backups run and how long they are kept; write it into [backups and restore](../../operations/data/backups-and-restore.md). Once a quarter restore the latest backup into a new database and time it: that time is the current RTO.
6. **Site and app.** Search Console → «Core Web Vitals» for mobile and desktop; Google Play Console → Android vitals for crash-free sessions.
7. **Cost.** Google Cloud Billing → Reports for the last three months, plus the other providers' invoices; the owner sets the budget and creates the budget alert.

## How they are used

- Every architecture review, code audit and new-feature design starts by reading this file. Missing targets or missing current values are the review's first finding; a feature on a hot path (catalogue, orders, site pages, 1C sync) states its expected load and checks it does not move P95 or P99 past the target.
- Alerts in Cloud Monitoring use these thresholds and notify the owner.
- Once a month compare the last 30 days with the table and update **Current**; a target missed twice in a row becomes a task, not a new number.
- A proposed row becomes confirmed when the owner accepts its number; then it gets its alert.
