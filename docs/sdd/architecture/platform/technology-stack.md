# Технологічний стек

| Область | Поточний вибір | Правило для нової роботи |
| --- | --- | --- |
| Runtime | .NET 10 / C#; `global.json` задає SDK `10.0.101` із `latestFeature` roll-forward | Дотримуйся nullable-анотацій і налаштованих аналізаторів. |
| HTTP | ASP.NET Core controllers | Залишай controllers тонкими й використовуй Mediator. |
| Mediator | Mediator 3.x | Використовуй пари request/handler і чинний pipeline. |
| Mediator pipeline | RequestLogging, Exception, Performance, Validation, DomainEventDispatcher behaviors | Зберігай порядок реєстрації; не розміщуй бізнес-правила в behaviors. |
| Валідація | FluentValidation | Розміщуй валідатор входу поряд із відповідним сценарієм використання. |
| Результат | Ardalis.Result | Повертай `Result` або `Result<T>` і застосовуй чинне API-відображення. |
| Дані | EF Core 10 + Npgsql/PostgreSQL | Конфігурація EF і міграції належать Infrastructure; виконання міграцій — окремому `Host.Jobs --job=migrate`. |
| Репозиторії | Ardalis.Specification | Повторно використовуй наявний стиль репозиторію/специфікацій там, де він застосовується. |
| Identity | ASP.NET Core Identity | Зберігай домовленості ролей, policies і fallback-auth. |
| Логи | Serilog | Лише структуровані логи без чутливих даних. |
| Фонова робота | Наявні абстракції фонових завдань та `Host.Jobs` | Не прив'язуй Application безпосередньо до планувальника; `Host.Jobs` є одноразовим CLI. |
| Зовнішній каталог | Інтеграція з 1C через SOAP | Тримай згенерований код і клієнта за адаптерами Infrastructure. |
| Тести | xUnit: unit, integration, architecture | Додавай тести відповідно до зміненої межі й ризику. |

Повний перелік **оголошених прямих** залежностей, версій і проєктів містить [каталог NuGet-пакетів](nuget-packages.md). Роль пакета у таблиці не доводить його фактичне використання без перевірки коду.

Не додавай пакет для заміни чинного підходу проєкту без явного рішення в специфікації фічі та погодження.
