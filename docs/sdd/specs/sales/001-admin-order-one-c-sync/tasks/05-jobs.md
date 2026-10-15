# Фаза 05 — фонова задача передавання

- [x] [T145 — сповіщення Telegram про dead-letter та експорт Excel](jobs/05.0-dead-letter-telegram.md)
- [x] [T150 — `SyncOneCOrdersJob`: надіслати й перевірити запити до 1С](jobs/05.1-sync-one-c-orders-job.md)
- [x] [T160 — розгортання та розклад Cloud Run Job](jobs/05.2-cloud-run-order-sync.md)

- [x] [T155 — зберегти номер документа 1С](jobs/05.3-persist-one-c-document-number.md)

## Контрольна точка

`Host.Jobs` може виконати `--job=sync-one-c-orders`; кожний запуск безпечно надсилає готові до передавання замовлення, оцінює синхронну відповідь 1С, фіксує стан передавання, один раз сповіщає Telegram, коли запис стає dead-letter, і розгортається як окрема Cloud Run Job.
