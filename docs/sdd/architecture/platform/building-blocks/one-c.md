# BuildingBlocks.Integrations.OneC

## Призначення

Цей проєкт зосереджує низькорівневий SOAP-механізм, спільний для інтеграцій з 1C.

## Структура

~~~text
BuildingBlocks.Integrations.OneC/
├── Factory/OneCSoapClientFactory
├── Auth/BasicAuthEndpointBehavior
├── DependencyInjection/AddOneCIntegrationServices
├── Generated/Reference.cs
└── Test/OneCSoapSmokeTest.cs
~~~

## Правила

- `OneCSoapClientFactory` реєструється як singleton через `AddOneCIntegrationServices`.
- `BasicAuthEndpointBehavior` додає базову автентифікацію до згенерованої кінцевої точки SOAP.
- `Generated/Reference.cs` — згенерований код; не рефактори його вручну.
- Клієнти 1C і перетворення даних конкретного модуля залишаються в Infrastructure цього модуля; цей проєкт дає лише спільну транспортну основу.
