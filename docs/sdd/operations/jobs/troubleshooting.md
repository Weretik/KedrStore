# Job troubleshooting

| Symptom | First checks | Do not do |
| --- | --- | --- |
| missing configuration | environment and `Host.Jobs` secrets | paste connection strings or SOAP credentials into logs |
| SOAP/auth failure | approved secret store and endpoint reachability | log headers, passwords or Authorization values |
| received zero records | root ID, environment, source availability | manual cleanup/repeated destructive imports |
| received non-zero, mapped zero | mapper, category lookup, response field format | rerun until data changes |
| public list stale | projection rebuild and `ExportToSite` | assume public invisibility equals deletion |
| stock list stale | `stocks` does not rebuild projection | assume list reads live product stock |
| locked DLL during build | stop local `Host.Api`, retry | delete output under a running host |

Retain job name, environment, root IDs when safe, timings, counts, exception type and database migration version. That supports diagnosis without retaining full SOAP payloads or secrets.
