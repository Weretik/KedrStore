# API читання замовлень менеджера — простежуваність сценаріїв

| Сценарій | Правила | Рівень тесту | Domain | Application | Infrastructure | API/контракт | Докази приймання | Статус |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001, R-005, R-006 | Application, API, інтеграція | — | EN-002, TS-002 | TS-001 | EN-003, TS-004, TS-005 | `SalesOrderReadModelTests.GetListAsync_ProjectsPersistedOrdersNewestFirst`; `ManagerOrderReadApiTests.GetList_ReturnsCompactPageAndPassesExactFilter` | перевірено |
| SC-002 | R-002, R-004, R-005 | Application, API, інтеграція | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `SalesOrderReadModelTests.GetByIdAsync_ReturnsSafeDetailForSoftDeletedCounterpartyWithoutChangingSync`; `ManagerOrderReadApiTests.GetById_ReturnsApprovedDetailAndDoesNotCallOneC` | перевірено |
| SC-003 | R-003, R-005 | Application, API, інтеграція | — | EN-002, TS-002 | TS-001 | EN-003, TS-004, TS-005 | `SalesOrderReadModelTests.GetListAsync_AppliesExactCounterpartyScopeAndPaging`; `ManagerOrderReadApiTests.GetList_ReturnsEmptyPageForUnknownCounterparty` | перевірено |
| SC-004 | R-002, R-005 | Application, API | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `GetOrderByIdQueryHandlerTests.Handle_PreservesNotFoundAndReadFailure`; `ManagerOrderReadApiTests.GetById_ReturnsNotFoundForUnknownOrder` | перевірено |
| SC-005 | R-005 | API | — | — | — | EN-003, TS-004, TS-005 | `ManagerOrderReadApiTests.ReadRoutes_AllowAnonymousAccess` | перевірено |

## Спільні підготовчі задачі

| Підготовча задача | Підтримує | Причина | Перевірка |
| --- | --- | --- | --- |
| EN-001 | SC-001–SC-005 | Погодити маршрути, межу DTO, авторизацію, семантику списку та видимість історії клієнта | Контрольний список готовності специфікації завершено до реалізації |
| EN-002 | SC-001–SC-004 | Визначити проєкції запитів без зв'язування Application з EF | Збірка Application і цільові тестові фікстури проходять |
| EN-003 | SC-001–SC-005 | Визначити версіонований HTTP-еталон до реалізації transport | Валідація Redocly, агреговані `$ref`, статичні твердження та перевірки runtime-маршрутів проходять |

## Примітки та обґрунтовані пропуски

- Задачі Domain не заплановано, оскільки всі сценарії запитують наявні агрегати без зміни доменної поведінки.
- Міграцію не заплановано; її відсутність потрібно переглянути лише тоді, коли погоджений показник продуктивності вимагатиме нового індексу.
