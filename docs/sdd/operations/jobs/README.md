# Фонові завдання й операційні інструкції з імпорту

`Host.Jobs` — консольний хост: складає DI, створює одну область дії, виконує названу операцію й завершується. У хості немає внутрішнього планувальника, черги або циклу повторного виконання. Окремі завдання можуть координуватися через збережений стан: наприклад, доставка замовлень Sales зберігає час наступної спроби й використовує оптимістичне керування конкурентністю в PostgreSQL.

```text
оператор / cron / CI → Host.Jobs (--job=...) → scoped Catalog/Sales job → PostgreSQL
```

- [CLI commands and configuration](host-jobs-cli.md)
- [Міграції й seed-дані](../data/migrations-and-seeding.md) — команда `--job=migrate`
- [Операційна інструкція Catalog/1C](catalog-one-c-runbook.md)
- [Операційна інструкція Sales/1C](sales-one-c-runbook.md)
- [Troubleshooting](troubleshooting.md)

Контракти й власність даних описані в [архітектурі інтеграції з 1C](../../architecture/integrations/one-c/README.md). Не вставляй секрети або реальні URL кінцевих точок у вивід, задачі чи документацію.
