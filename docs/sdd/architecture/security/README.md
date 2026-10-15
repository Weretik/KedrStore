# Identity, автентифікація й авторизація

Це архітектурний розділ для облікових записів, автентифікації, авторизації та планування контролю доступу. Він розміщений окремо від `platform`: платформа описує технології, а тут описано їх використання застосунком.

```text
security/
├── authentication/  login, bearer tokens, refresh, logout і CSRF
├── authorization/   fallback-захист, ролі й названі policies
├── users/           початкові користувачі, імпорт і дизайн реєстрації
└── planning/        SDD-чеклист фічі з контролем доступу
```

## Навігація за завданням

- Проблема сесії, токена чи refresh: [автентифікація](authentication/README.md)
- Захист кінцевої точки, додавання правила авторизації чи ролі: [авторизація](authorization/README.md)
- Створення користувача, реєстрація чи запрошення: [користувачі](users/README.md)
- Планування фічі з дозволами: [чеклист](planning/access-control-checklist.md)

## Відповідальність

```text
Identity.Api             HTTP-сесія та майбутні endpoints Identity
        |
Identity.Application     commands/queries, validators і контракти сервісів
        |
Identity.Infrastructure  ASP.NET Core Identity, токени, сесії, AppIdentityDbContext
        |
Identity.Domain          назви ролей

Host.Api                 authentication middleware та authorization policies
```

Identity відповідає за облікові записи й облікові дані. Бізнес-модуль може захищати свою кінцеву точку через правило авторизації, але не створює користувачів, не перевіряє паролі й не звертається безпосередньо до `UserManager<AppUser>`.
