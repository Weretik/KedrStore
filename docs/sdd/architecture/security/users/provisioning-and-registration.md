# Користувачі, початкове створення й реєстрація

## Що існує зараз

У поточному API немає кінцевої точки публічної самореєстрації. API сесії підтримує лише вхід, поновлення, вихід і `me`.

Поточні шляхи створення користувачів:

| Шлях | Поведінка |
| --- | --- |
| Окремий `Host.Jobs --job=migrate` | `RoleSeeder` створює визначені ролі. `IdentitySeeder` створює налаштованого адміністратора лише якщо його ще немає й надано `ADMIN_DEFAULT_PASSWORD`. Звичайний старт `Host.Api` сидери не запускає. |
| Імпорт контрагента | `ImportedCounterpartyIdentityProvisioningService` знаходить або створює `AppUser`, синхронізує його поля/пароль і призначає роль `User`, якщо вона існує. |

Початкового адміністратора налаштовує `AdminUserOptions`; пароль читається з `ADMIN_DEFAULT_PASSWORD`. Ніколи не коміть це значення до конфігурації чи документації.

## Дизайн нової реєстрації або керування користувачами

Розглядай це як фічу модуля Identity, а не деталь реалізації Catalog, Sales чи API controller.

```text
Identity.Api endpoint
  -> Identity.Application command + validator
  -> application service contract
  -> Identity.Infrastructure implementation
  -> UserManager<AppUser> / RoleManager<AppRole> / AppIdentityDbContext
```

Мінімальні рішення для специфікації фічі:

- Хто створює користувача: анонімна самореєстрація, лише Admin, лише імпорт або інша policy.
- Поля Identity й валідація: унікальність email, нормалізація входу, повне ім'я та правила пароля.
- Початкова роль і можливість її вибору викликачем. Публічний запит ніколи не може надавати Admin чи Manager.
- Активація: негайний login, підтвердження email, запрошення або погодження адміністратора.
- Налаштування/скидання пароля й відновлення доступу.
- Захист від зловживань: rate limit, audit logs і потреба в CAPTCHA/підтвердженні email.
- Доля активних сесій після зміни ролі, пароля або стану облікового запису.
- Форма API-відповіді: ніколи не повертати пароль, refresh token, password-reset token або внутрішні помилки Identity.

## Межі реалізації

- HTTP-контракт і controller розміщуй в `Identity.Api`.
- Команда або запит, валідатор FluentValidation, контракт `Ardalis.Result` та інтерфейс для Application — в `Identity.Application`.
- Виклики ASP.NET Core Identity, збереження `AppUser`, призначення ролей і виклики провайдера токенів — в `Identity.Infrastructure`.
- В `Identity.Domain` залишай лише сталі константи ролей; не клади туди HTTP DTO, `UserManager` або EF-залежності.
- Новий сервіс реєструй чинним шляхом модуля Identity; `Host.Api` складе його через `AddHostServices`.

За помилками результату `UserManager` формуй безпечний і послідовний результат Application. Не відкривай сирі діагностичні дані Identity анонімним викликачам.
