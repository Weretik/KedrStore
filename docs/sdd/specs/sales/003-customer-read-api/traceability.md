# API читання клієнтів — зв'язок сценаріїв із завданнями

| Сценарій | Правила | Рівень тестів | Домен | Application | Інфраструктура | API/контракт | Свідчення приймання | Статус |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001, R-002, R-005, R-006 | Application, API, інтеграція | — | EN-002, TS-002 | TS-001 | EN-003, TS-004, TS-005 | `SalesCustomerReadModelTests.GetListAsync_ReturnsOnlyActiveCustomersOrderedByNameAndId`; `GetListAsync_AppliesPagingAndReturnsEmptyPagePastTheEnd`; `AdminCustomerApiTests.GetList_ReturnsCompactPageAndUsesPagingDefaults` | verified |
| SC-002 | R-001, R-003, R-004, R-005 | Application, API, інтеграція | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `SalesCustomerReadModelTests.GetByIdAsync_ReturnsActiveCustomerAndOrderedCategoryPriceTypes`; `AdminCustomerApiTests.GetById_ReturnsApprovedDetailWithoutPrivateOrOrderData` | verified |
| SC-003 | R-002, R-005 | Application, API | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `SalesCustomerReadModelTests.GetByIdAsync_ReturnsNotFoundForUnknownAndSoftDeletedCustomers`; `AdminCustomerApiTests.GetById_ReturnsNotFoundForUnavailableCustomer` | verified |
| SC-004 | R-005 | API | — | — | — | EN-003, TS-004, TS-005 | `AdminCustomerApiTests.ReadRoutes_ReturnSuccessWithoutAuthentication` підтверджує роботу списку й деталей | verified |

## Спільні передумови

| Передумова | Забезпечує | Причина | Перевірка |
| --- | --- | --- | --- |
| EN-001 | SC-001–SC-004 | Погодити дизайн маршрутів, анонімний доступ, межу приватності відповіді, семантику списку й поведінку для видаленого клієнта | Контрольний список готовності специфікації та зміна щодо анонімного доступу завершені |
| EN-002 | SC-001–SC-003 | Створити проєкції запитів з обмеженим набором даних без залежності Application від EF | Збірка Application і цільові фікстури обробників пройшли |
| EN-003 | SC-001–SC-004 | Визначити версійний еталон HTTP перед реалізацією транспортного шару | Валідація Redocly, зведене посилання `$ref`, статичні перевірки та перевірки маршрутів під час виконання пройшли |

## Примітки й обґрунтовані пропуски

- Завдання для домену не заплановано, оскільки всі сценарії читають наявні агрегати без зміни доменної поведінки.
- Міграцію не заплановано; переглянути це рішення потрібно лише за виміряної потреби в індексуванні для погодженої поведінки списку.
