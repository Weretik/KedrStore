# Локальний запуск Host.Api

## Передумови

- .NET SDK для цільового `net10.0`; точну базову версію й roll-forward визначає `global.json`.
- PostgreSQL, доступний через `ConnectionStrings:Default`, зі схемою, яку підготовлено окремим завданням міграції.
- Потрібні локальні секрети; див. [конфігурацію й секрети](../configuration/configuration-and-secrets.md).

## Запуск

Із кореня репозиторію:

~~~powershell
dotnet restore KedrStore.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
~~~

Профіль `https` слухає `https://localhost:7230` і `http://localhost:5081`. Для перевірки автентифікації та refresh використовуй HTTPS: поточний refresh-cookie має атрибут `Secure`.

Для перевірки кінцевої точки лише через HTTP використовуй профіль `http`:

~~~powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
~~~

API не запускає міграції або сидери під час старту. Підготовку БД виконує окрема команда `Host.Jobs --job=migrate`; перевір її порядок і наслідки в [інструкції з міграцій і seed-даних](../data/migrations-and-seeding.md). API однаково потребує налаштованого доступу до БД для робочих запитів.

## Зупинка й повторна збірка

Якщо Windows блокує вихідні DLL, зупини запущений `Host.Api` перед збіркою. Далі виконай потрібні команди зі [стандарту тестування](../../standards/testing-rules.md).
