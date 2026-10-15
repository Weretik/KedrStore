# Інструменти збірки, перевірки й доставки

**Факт, перевірено 2026-10-15:** цей документ звірено з `global.json`, `KedrStore.sln`, `.github/workflows/deploy-cloudrun.yml`, файлами Dockerfile, `Program.cs` обох хостів і налаштуваннями запуску. Наявність кроку в процесі CI не доводить успішного розгортання в конкретне середовище.

| Область | Джерело в репозиторії | Поточне використання |
| --- | --- | --- |
| SDK | `global.json` | .NET SDK `10.0.101`, `rollForward: latestFeature`; проєкти орієнтовані на `net10.0`. |
| Solution | `KedrStore.sln` | Єдина точка для `dotnet restore`, `dotnet build` і загального `dotnet test`. |
| Локальні профілі API | `src/Bootstrapper/Host.Api/Properties/launchSettings.json` | Профілі `http` і `https`; адреси й спосіб запуску — в [операційній інструкції](../../operations/local-development/run-api.md). |
| Конфігурація | `src/Bootstrapper/Host.Api/appsettings*.json`, `src/Bootstrapper/Host.Jobs/Host.Jobs/appsettings.json`, ASP.NET Core User Secrets, змінні середовища | Секрети та джерела — у [конфігурації](../../operations/configuration/configuration-and-secrets.md). `Host.Jobs` у Development використовує власні User Secrets. |
| Контейнери | `src/Bootstrapper/Host.Api/Dockerfile`, `src/Bootstrapper/Host.Jobs/Host.Jobs/Dockerfile` | Окремі образи для HTTP host та CLI jobs. |
| CI | `.github/workflows/deploy-cloudrun.yml`, завдання `verify` | Отримання коду, .NET із `global.json`, відновлення залежностей, збірка Release, UnitTests, ArchitectureTests, IntegrationTests і завантаження покриття у форматі Cobertura. IntegrationTests можуть потребувати Docker для PostgreSQL Testcontainers. |
| Доставка | `.github/workflows/deploy-cloudrun.yml`, job `deploy` | Після успішної перевірки створює образи, запускає Cloud Run `database-migrate` job із `--job=migrate` й очікує завершення, тоді розгортає `Host.Api` і оновлює образи jobs. Workflow запускається на push до `master`. |

Локальні команди, визначені [правилами тестування](../../standards/testing-rules.md):

```powershell
dotnet restore KedrStore.sln
dotnet build KedrStore.sln --no-restore
dotnet test KedrStore.sln --no-build
```

`Host.Jobs` є одноразовим CLI: `--job=<name>` вибирає операцію, `--job=migrate` виконує міграції й сидери. Внутрішній планувальник не запускає завдання самостійно. Див. [CLI та операційні інструкції](../../operations/jobs/README.md). Не запускай завдання міграції за самим фактом читання цієї документації: команда змінює дані та схему.

Під час зміни SDK, маніфестів, файлів Dockerfile, процесу CI або команди запуску онови цей опис, [стек](technology-stack.md), [каталог пакетів](nuget-packages.md) і відповідні операційні інструкції.
