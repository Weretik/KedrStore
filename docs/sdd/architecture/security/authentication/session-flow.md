# Автентифікація

## Поточний механізм

KedrStore використовує ASP.NET Core Identity зі схемою bearer token (`IdentityConstants.BearerScheme`). Дані Identity й записи refresh-сесій зберігаються через `AppIdentityDbContext` у PostgreSQL. `Host.Api` вмикає authentication middleware перед authorization.

```text
Client
  | POST /api/auth/session/login { email, password }
  v
AuthSessionController (AllowAnonymous, AuthLogin rate limit)
  v
SessionLoginCommand -> Mediator behaviors -> IdentitySessionService
  v
UserManager + SignInManager -> AppUserClaimsPrincipalFactory
  v
access token in JSON response + refresh token in HttpOnly cookie
```

`AppUserClaimsPrincipalFactory` додає user ID, ім'я, email, security stamp, claims користувача й ролі до автентифікованого principal. Тому ролі доступні authorization policies.

## Кінцеві точки сесії

| Endpoint | Доступ | Вхід і результат |
| --- | --- | --- |
| `POST /api/auth/session/login` | `AllowAnonymous`, `AuthLogin` rate limit | Email/password. Повертає тип токена, access token і строк дії; записує refresh та CSRF cookies. |
| `POST /api/auth/session/refresh` | `AllowAnonymous`, `AuthRefresh` rate limit | Потребує refresh-token cookie і налаштованого CSRF header, значення якого збігається з CSRF cookie. Ротує refresh-сесію й повертає новий access token. |
| `POST /api/auth/session/logout` | Автентифікований користувач | Відкликає активні refresh-сесії, оновлює security stamp, очищає refresh і CSRF cookies. |
| `GET /api/auth/session/me` | Автентифікований користувач | Повертає поточні user ID, email і ролі. |

Refresh token ніколи не повертається в JSON-відповіді. Його cookie має `HttpOnly`; CSRF cookie доступний browser, щоб frontend передав його значення в налаштованому header під час refresh.

## Правила токенів і сесій

- Строки дії access і refresh токенів беруться з `IdentitySessionSecurityOptions` під час реєстрації `AddBearerToken`.
- Кожен refresh token зберігається в `RefreshSessions` лише як SHA-256 hash разом із часовими позначками та обмеженим контекстом клієнта.
- Сеанси поновлення мають абсолютне завершення строку дії та завершення за відсутності активності.
- Успішний refresh ротує сесію: стару відкликає і пов'язує з новою.
- Повторне використання відкликаного або заміненого refresh token відкликає всі активні refresh-сесії користувача.
- Вихід із системи також змінює мітку безпеки користувача, роблячи недійсними токени поновлення зі старою міткою.

## Поточні параметри Identity

- Електронна адреса має бути унікальною.
- Пароль має щонайменше 5 символів.
- Цифри, малі й великі літери та неалфавітно-цифрові символи наразі не обов'язкові.

Це поточні runtime-налаштування, а не рекомендація для фічі публічної реєстрації. Перед її реалізацією потрібно явно переглянути політику пароля, підтвердження email, захист від зловживань і відновлення доступу.

## Використання у frontend

1. Виклич `login` і зберігай отриманий access token згідно з дизайном безпеки frontend.
2. Передавай його як `Authorization: Bearer <access-token>` у захищених API-запитах.
3. Під час refresh дозволь browser надсилати cookies і додай налаштований CSRF header зі значенням CSRF cookie.
4. Під час logout виклич `logout`, доки access token чинний, потім видали його на клієнті.

Не клади refresh token у local storage, логи застосунку чи тіло frontend-запиту.
