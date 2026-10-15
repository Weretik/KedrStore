# Операційна документація

Розділ пояснює, як налаштувати, запустити й діагностувати backend. Тут описано операційні факти; ручні перевірки конкретної фічі зберігаються в її специфікації.

```text
operations/
├── local-development/  запуск API та локальні адреси
├── configuration/      джерела конфігурації й секрети
├── data/               окремий migration job і seed-дані
├── diagnostics/        health, Swagger і логи
└── jobs/               команди Host.Jobs та імпорту 1C
```

## Навігація за завданням

- Локально запустити backend: [локальна розробка](local-development/run-api.md)
- Налаштувати середовище: [конфігурація й секрети](configuration/configuration-and-secrets.md)
- Підготувати БД окремим job: [міграції та seed-дані](data/migrations-and-seeding.md)
- Перевірити API або відтворити HTTP-запит: [діагностика](diagnostics/health-logs-swagger.md)
- Виконати 1C/import операцію: [jobs і runbooks](jobs/README.md)

Не записуй тут паролі, токени, повні рядки підключення або production endpoints.
