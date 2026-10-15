# Конфігурація й секрети

## Джерела

`Host.Api` використовує стандартні джерела конфігурації ASP.NET Core. Для Development у його проєкті задано User Secrets ID; локальні чутливі значення зберігай у User Secrets. `Host.Jobs` також завантажує власні User Secrets у Development, окремі від API. Під час розгортання передавай чутливі значення через погоджений механізм секретів/змінних середовища.

У ключах змінних середовища використовуй подвійне підкреслення: наприклад, `ConnectionStrings__Default` відповідає `ConnectionStrings:Default`.

## Важливі параметри

| Параметр | Призначення | Секрет |
| --- | --- | --- |
| `ConnectionStrings:Default` | З'єднання PostgreSQL для контекстів застосунку | так |
| `ADMIN_DEFAULT_PASSWORD` | Пароль лише для початкового створення адміністратора через migration job | так |
| `OneCSoap:Endpoint`, `Username`, `Password` | SOAP-інтеграція з 1C | кінцева точка залежить від середовища; облікові дані — секрет |
| `Telegram:BotToken`, `ChatId` | Інтеграція з Telegram | так |
| `Sales:OneCOrderSync` | Розмір batch, інтервали, межа спроб, київське вікно відправлення та recovery застарілого `Sent` | ні |
| `Cors:AllowedOrigins` / `AllowedOriginsCsv` | Дозволені browser origins для API | ні, але залежить від середовища |
| `Identity:SessionCookies` | Назви, шляхи, строк життя, `Secure` та SameSite для cookies | ні |
| `Identity:SessionSecurity` | Межі строку дії access/refresh токенів | ні |

Приклад локального секрету для `Host.Api`:

~~~powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-only-value>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
~~~

Для `Host.Jobs --job=migrate` надай `ADMIN_DEFAULT_PASSWORD` і `ConnectionStrings:Default` через **його** User Secrets або змінні середовища. API User Secrets автоматично не передаються Jobs host.

Не записуй реальні паролі, токени чи production-рядки підключення в `appsettings*.json`, Markdown, код або логи. Під час діагностики запуску не друкуй значення конфігурації.

## Файли cookie й CORS

Refresh-автентифікація використовує cookies з credentials, тому CORS задає явний allow-list; wildcard origins не використовуються. Поточний refresh-cookie має `Secure: true` і `SameSite: None`. Перевіряй refresh через HTTPS або зроби свідоме локальне перевизначення, яке не переноситься в спільне середовище.
