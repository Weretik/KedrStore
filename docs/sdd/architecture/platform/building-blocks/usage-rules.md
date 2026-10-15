# Правила використання BuildingBlocks

## Перевикористання перед створенням

| Потреба | Спершу застосуй | Не роби |
| --- | --- | --- |
| Очікувана помилка сценарію використання | Ardalis.Result і чинне відображення Result | Кидати звичайні винятки валідації або відсутності даних |
| Валідація входу | FluentValidation і ValidationBehavior | Валідувати лише в controller |
| Бізнес-інваріант | DomainError і DomainException або поведінка aggregate | Переносити доменне правило в behavior |
| Поточний користувач/дозволи | ICurrentUserService або IPermissionService | Інжектити IHttpContextAccessor в Application |
| Доменна подія | Колекція BaseEntity і dispatcher behavior | Публікувати безпосередньо з controller |
| Міграція/seed БД | IDatabaseMigrator/ISeeder через окремий `Host.Jobs --job=migrate` | Виконувати міграції/сидери всередині handler |
| 1C SOAP | OneCSoapClientFactory | Дублювати налаштування SOAP-автентифікації |
| Діагностика запитів | Наявні behaviors/log helpers | Логувати сирі чутливі payloads |

## Правило розширення

Додавай спільний примітив лише коли щонайменше два модулі потребують однакової сталої технічної абстракції. Абстракція окремої фічі залишається в її Application-модулі. Бізнес-поняття не переносять до BuildingBlocks.

Додавання або зміну порядку наскрізної інфраструктури документуй в [ADR](../../decisions/README.md) і специфікації фічі.
