# BuildingBlocks.Application

## Структура

~~~text
BuildingBlocks.Application/
├── Behaviors/
│   ├── RequestLoggingBehavior
│   ├── ExceptionBehavior
│   ├── PerformanceBehavior + параметри
│   ├── ValidationBehavior
│   └── DomainEventDispatcherBehavior
├── Contracts/
│   ├── ICurrentUserService, IPermissionService
│   ├── ICacheService, IEmailService, IEnvironmentService
│   └── технічні абстракції для модулів
├── Notifications/
│   ├── IDomainEventContext, IDomainEventDispatcher
│   └── DomainEventNotification + handler
├── Logging/                  згенеровані структуровані log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
~~~

## Поведінки pipeline

Host реєструє RequestLogging, Exception, Performance, Validation і DomainEventDispatcher саме в такому порядку. Точна семантика — у [Mediator behaviors](../../runtime/mediator-behaviors.md).

## Сповіщення

`IDomainEventContext` надає сутності з накопиченими подіями. `IDomainEventDispatcher` публікує події. `DomainEventNotification` є обгорткою повідомлення Mediator. Типовий handler повідомлення навмисно порожній; handlers конкретних модулів підписуються на цей шаблон повідомлень/подій за потреби.

## Логи й helpers

Папка Logging містить сталі source-generated шаблони Serilog/Microsoft для запитів, валідації, продуктивності, доменних подій, cancellation і необроблених збоїв. Використовуй їх через чинні behaviors; не логуй сирі чутливі дані запиту.

`PhoneNumberHelper` — спільний інструмент нормалізації та перевірки телефонів. Використовуй його замість дублювання parsing.

## Доступність контрактів

`ICurrentUserService` та `IPermissionService` мають спільні реалізації в Infrastructure. `ICacheService`, `IEmailService` і `IEnvironmentService` наразі є лише контрактами в BuildingBlocks; спільну реалізацію там не зареєстровано. Не інжекти їх у нову фічу, доки відповідальний модуль не надасть і не зареєструє реалізацію.
