# Background Job Troubleshooting

| Symptom | First checks | Avoid |
| --- | --- | --- |
| Missing configuration | `Host.Jobs` environment and secrets | Putting connection strings or SOAP credentials in logs |
| SOAP/authentication failure | Approved secret store and endpoint availability | Logging headers, passwords, or Authorization values |
| Zero records received | Root ID, environment, and source availability | Manually clearing data or repeating a destructive import |
| Records received, but mapped = 0 | Mapper, category lookup, response field format | Repeating the job and hoping the data changes |
| Public list is stale | Projection rebuild and `ExportToSite` | Treating absence from the list as deletion |
| Stock list is stale | Whether `stocks` and projection rebuild completed; check logs and `ProductListProjections` | Assuming the list reads live product stock |
| DLL locked during build | Stop local `Host.Api` and retry | Deleting output while the host is running |

Record the job name, environment, safe root IDs, duration, counters, exception type, and database migration version. This supports diagnosis without full SOAP payloads or secrets.
