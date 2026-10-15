# Структура API та host

## Структура API-модуля

~~~text
<Module>.Api/
├── Controllers/
│   └── <Area>Controller.cs
├── GlobalUsing.cs
└── <Module>.Api.csproj
~~~

Controllers прив'язують HTTP-вхід, викликають `ISender.Send` із `CancellationToken` і повертають результат через чинне відображення Result. Вони не виконують EF-запитів, не створюють репозиторіїв і не містять доменних правил.

## Структура host

~~~text
Bootstrapper/Host.Api/
├── Program.cs
├── DependencyInjection/
│   ├── ServiceRegistration/
│   │   ├── ApplicationRegistrationExtensions.cs
│   │   ├── Pipeline/
│   │   ├── SecurityRegistrationExtensions.cs
│   │   └── Web/
│   └── WebApplication/
└── appsettings*.json
~~~

## Конвеєр запиту

~~~text
HTTP
  → обробка винятків / діагностика Development
  → CORS, HTTPS, Serilog request logging, rate limiter
  → автентифікація → авторизація
  → controller → ISender
  → логування запиту → винятки → продуктивність → валідація → доменні події
  → handler → HTTP-відображення Result
~~~

Хост — корінь композиції. Він реєструє модулі й проміжне ПЗ, але не реалізує бізнес-сценарій. `Host.Api` не виконує міграції при старті; їх запускає окреме [завдання міграції](../../operations/data/migrations-and-seeding.md).
