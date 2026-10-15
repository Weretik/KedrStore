# Інтеграція з 1C

Розділ фіксує сталу межу між KedrStore і 1C. Він описує поточну поведінку коду без credentials, endpoints або реальних production-ідентифікаторів.

```text
1C SOAP → BuildingBlocks.Integrations.OneC → module adapter → Application job/service → database
                                   │                         │
                              лише WCF/auth            відповідальність Catalog або Sales
```

## Навігація за завданням

- SOAP-операції й нормалізація: [контракт клієнта](client-contract.md)
- Порядок імпорту Catalog, проєкція й видимість: [потік імпорту](catalog-import-flow.md)
- Терміни Catalog, зокрема Doors, Hardware і Cosmos: [продуктовий словник](../../../../product/glossary.md)
- Імпорт контрагентів і створення користувачів Identity: [потік Sales/Identity](sales-counterparty-identity-sync.md)
- Семантика видалення й безпечне виконання: [безпека даних](data-safety.md)
- Запуск і діагностика jobs: [runbooks Host.Jobs](../../../operations/jobs/README.md)

## Межі відповідальності

- `BuildingBlocks.Integrations.OneC` володіє згенерованим транспортом WCF і базовою автентифікацією. `Generated/Reference.cs` — згенерований код, не редагуй його вручну.
- Інфраструктурні шари Catalog і Sales відповідають за адаптери й перетворення даних власних модулів.
- Прикладний шар Catalog координує завдання через `IOneCClient` і не залежить від WCF-типів.
- `Host.Jobs` — консольний хост одного запуску без власного планувальника; його викликає cron, CI або автоматизація розгортання.

Контракт, власність даних, безпека й операції описані окремо, щоб зміна mapping поля не стала непомітно руйнівною зміною даних.
