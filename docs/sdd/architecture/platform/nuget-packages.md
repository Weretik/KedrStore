# Прямі NuGet-залежності

**Перевірено 2026-10-15:** усі `src/**/*.csproj` і `tests/**/*.csproj` у репозиторії. Виявлено 68 записів `PackageReference`, що утворюють 53 унікальні пари «назва + оголошена версія». `Directory.Packages.props` немає; версії задано в самих `.csproj`. Таблиця описує **оголошені прямі залежності**, а не гарантує, що кожен пакет фактично викликається кодом або встановлений у конкретному середовищі. Для транзитивних залежностей і розв'язаних версій використовуй NuGet restore/assets, не додавай їх вручну до цього каталогу.

Позначення у стовпці «Проєкт»: `B.*` = `src/BuildingBlocks/BuildingBlocks.*`, `C.*` = `src/Catalog/Catalog.*`, `S.*` = `src/Sales/Sales.*`, `H.Api` = `src/Bootstrapper/Host.Api/Host.Api.csproj`, `T.Unit` = `tests/UnitTests/UnitTests.csproj`, `T.Integration` = `tests/IntegrationTests/IntegrationTests.csproj`, `T.Architecture` = `tests/ArchitectureTests/ArchitectureTests.csproj`. Для `B.*`, `C.*` і `S.*` до назви проєкту додай `.csproj`; для `B.Integrations.OneC` шлях `src/BuildingBlocks/BuildingBlocks.Integrations.OneC/BuildingBlocks.Integrations.OneC.csproj`. Кожен запис таблиці можна звірити з указаним manifest. Однаковий пакет у кількох проєктах наведено одним рядком із усіма власниками.

| Пряма залежність | Версія | Проєкт | Призначення або перевірний контекст |
| --- | --- | --- | --- |
| `Ardalis.Result` | 10.1.0 | B.Api, C.Contracts | Типізований результат і контракт відповіді; HTTP-відображення описано в [BuildingBlocks.Api](building-blocks/api.md). |
| `Ardalis.Result.AspNetCore` | 10.1.0 | B.Application | Інтеграція `Result` з ASP.NET Core; перевіряй точне використання у коді перед зміною. |
| `Ardalis.Result.FluentValidation` | 10.1.0 | B.Application | Інтеграція помилок `FluentValidation` з `Result`; перевіряй точне використання у коді. |
| `Ardalis.SmartEnum` | 8.2.0 | C.Domain | Типізовані перелічення в домені Catalog. |
| `Ardalis.Specification` | 9.3.1 | B.Application | Специфікації для репозиторних запитів; [підхід до збереження даних](../runtime/persistence-and-integrations.md). |
| `Ardalis.Specification.EntityFrameworkCore` | 9.3.1 | B.Application | EF Core-адаптер специфікацій; фактичні виклики перевіряй у коді. |
| `ClosedXML` | 0.105.0 | B.Application | Створення Excel-файлів; перевіряй відповідальність шару та використання в експорті. |
| `FluentValidation` | 12.1.1 | B.Application | Валідатори вхідних даних у use cases. |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | B.Application | Реєстрація валідаторів у DI; фактична реєстрація у `Host.Api/DependencyInjection/ServiceRegistration/Pipeline/FluentValidationRegistrationExtensions.cs`. |
| `Hangfire` | 1.8.22 | B.Application | Оголошена бібліотека фонового виконання; поточний `Host.Jobs` працює як одноразовий CLI без власного планувальника ([опис](../../operations/jobs/README.md)). Не приписуй їй виконання завдань без перевірки коду. |
| `libphonenumber-csharp` | 9.0.22 | B.Application, S.Infrastructure | Обробка й перевірка телефонних номерів; шукай конкретні виклики у відповідних проєктах. |
| `Mediator.Abstractions` | 3.0.1 | B.Application | Контракти request/handler і pipeline Mediator. |
| `Mediator.SourceGenerator` | 3.0.1 | H.Api | Генерація реалізації Mediator під час збірки host; реєстрація у `MediatorRegistrationExtensions.cs`. |
| `Microsoft.AspNetCore.Http.Abstractions` | 2.3.9 | B.Application, B.Infrastructure | HTTP-абстракції, оголошені у двох BuildingBlocks-проєктах; перевіряй актуальну потребу перед зміною. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.2 | B.Application | ASP.NET Core Identity з EF Core; конкретне використання та межі шару звіряй у коді. |
| `Microsoft.AspNetCore.OpenApi` | 10.0.2 | H.Api | Вбудована кінцева точка OpenAPI; `Host.Api/Program.cs` викликає `MapOpenApi()` у Development. |
| `Microsoft.EntityFrameworkCore` | 10.0.2 | B.Application | EF Core API, оголошений у BuildingBlocks.Application; контексти й міграції належать Infrastructure. |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.2 | H.Api | Інструменти design-time для EF Core; не runtime-сервіс. |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.2 | B.Application, B.Integrations.OneC | Інтерфейси конфігурації для спільних компонентів та 1C. |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.2 | B.Application | Інтерфейси DI для реєстрації й резолюції сервісів. |
| `Microsoft.Extensions.Http` | 10.0.2 | B.Application | `HttpClientFactory` та DI для HTTP-клієнтів; конкретні адаптери перевіряй у коді. |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.2 | B.Application | Інтерфейси logging для application-компонентів. |
| `Microsoft.OpenApi` | 2.8.0 | H.Api | Об'єктна модель OpenAPI для конфігурації схем і Swagger. |
| `Newtonsoft.Json` | 13.0.4 | B.Application | Оголошений JSON serializer; точну потребу й місця виклику звіряй до зміни. |
| `NodaMoney` | 2.6.0 | B.Application, C.Domain | Грошові значення та валюти для контрактів/домену Catalog. |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.0 | B.Application | EF Core provider PostgreSQL; фактичні `DbContext` та міграції в Infrastructure. |
| `Scriban` | 7.2.7 | H.Api | Шаблонізатор, оголошений у API host; поточні виклики й потребу перевіряй у коді. |
| `Serilog` | 4.3.0 | H.Api | Структуроване журналювання API host. |
| `Serilog.AspNetCore` | 10.0.0 | H.Api | ASP.NET Core інтеграція Serilog; `Program.cs` викликає request logging. |
| `Serilog.Enrichers.Environment` | 3.0.1 | H.Api | Додаткові властивості середовища у логах за конфігурацією. |
| `Serilog.Enrichers.Process` | 3.0.0 | H.Api | Додаткові властивості процесу у логах за конфігурацією. |
| `Serilog.Settings.Configuration` | 10.0.0 | H.Api | Читання налаштувань Serilog з конфігурації. |
| `Slugify.Core` | 5.1.1 | B.Application, T.Unit | Формування URL-slug і його unit-перевірки. |
| `Swashbuckle.AspNetCore` | 10.0.0 | H.Api | Генерація Swagger UI та OpenAPI у Development. |
| `System.Security.Cryptography.Xml` | 10.0.10 | B.Integrations.OneC | XML-криптографія, оголошена для 1C-інтеграції; конкретний протокол перевіряй за адаптером. |
| `System.ServiceModel.Duplex` | 6.0.0 | B.Integrations.OneC | WCF duplex API, оголошений 1C-проєктом; фактичну потребу перевіряй. |
| `System.ServiceModel.Federation` | 10.0.652802 | B.Integrations.OneC | WCF federation API, оголошений 1C-проєктом; фактичну потребу перевіряй. |
| `System.ServiceModel.Http` | 10.0.652802 | B.Integrations.OneC | Транспорт HTTP для SOAP/WCF-клієнта 1C. |
| `System.ServiceModel.NetTcp` | 10.0.652802 | B.Integrations.OneC | WCF NetTcp API, оголошений 1C-проєктом; фактичну потребу перевіряй. |
| `System.ServiceModel.Primitives` | 10.0.652802 | B.Integrations.OneC | Базові WCF контракти для SOAP-клієнта. |
| `System.ServiceModel.Security` | 6.0.0 | B.Integrations.OneC | WCF security API для SOAP-клієнта; конкретне застосування перевіряй. |
| `Telegram.Bot` | 22.8.1 | B.Application | Транспорт Telegram для сповіщень; [операційний контекст](../../operations/jobs/sales-one-c-runbook.md). |
| `Unidecode.NET` | 2.2.1 | B.Application | Транслітерація тексту; конкретне використання перевіряй у коді. |
| `Vogen` | 8.0.4 | B.Application, C.Domain | Оголошений генератор value objects для legacy-коду; [чинне правило](../../standards/identifier-strategy.md) не дозволяє нові typed IDs через Vogen. |

## Прямі пакети тестових проєктів

| Пряма залежність | Версія | Проєкт | Призначення |
| --- | --- | --- | --- |
| `coverlet.collector` | 6.0.4 | T.Unit, T.Integration, T.Architecture | Збір покриття тестів. |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.2 | T.Integration | Тестовий запуск ASP.NET Core host. |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.2 | T.Integration | Нереляційний EF provider лише для обмежених тестових сценаріїв; [правила](../../standards/testing-rules.md). |
| `Microsoft.NET.Test.Sdk` | 18.0.1 | T.Unit, T.Integration, T.Architecture | Інтеграція тестів з `dotnet test`. |
| `NetArchTest.Rules` | 1.3.2 | T.Architecture | Перевірки залежностей і архітектурних меж. |
| `Testcontainers.PostgreSql` | 4.15.0 | T.Integration | Тимчасовий PostgreSQL для інтеграційних тестів. |
| `xunit` | 2.9.3 | T.Unit, T.Integration, T.Architecture | Тестовий framework. |
| `xunit.runner.visualstudio` | 3.1.5 | T.Unit, T.Integration, T.Architecture | Запуск xUnit у test runner. |
| `Xunit.SkippableFact` | 1.4.13 | T.Integration | Явне пропускання тесту, коли локальна передумова недоступна. |

Версію SDK задає [`global.json`](../../../../global.json): `10.0.101` із `rollForward: latestFeature`; цільові frameworks у `.csproj` — `net10.0`. При оновленні залежностей звіряй цю таблицю з усіма manifests, а фактичне використання — з imports, DI та runtime-конфігурацією. Пакет, що лише оголошений, не називай обов'язковим архітектурним підходом без такого підтвердження.
