# 007 — Звіряння клієнтів Sales із повним списком 1C

- **Модуль:** Sales, інтеграція 1C; пов'язаний локальний користувач Identity
- **Статус:** звіряння й durable Identity deletion queue реалізовано; PostgreSQL/FK та concurrency сценарії ще не підтверджено тестами
- **Створено:** 2026-10-22
- **Спільний продуктовий інкремент:** `order-workspace-2026-10-22`; Admin frontend використовує актуальний список клієнтів через чинний `getAdminCustomers`

## Мета та межі

Кожний **успішний повний** імпорт клієнтів із 1C має привести локальний Sales до поточного складу джерела: нових додати один раз, наявних оновити за сталим `CounterpartyId`, відсутніх фізично видалити разом із локальними замовленнями й чернетками. Повторний прохід тієї самої відповіді не створює дублікати. Синхронізація локально читає 1C; не змінює записи клієнтів у 1C. Нового HTTP endpoint не потрібно; `GET /api/admin/customers` читає результат наявної job.

## Перевірений базис

- `Host.Jobs --job=counterparties` викликає `OneCCounterpartiesSyncService`; `--job=sales-customers-full` запускає його перед синхронізацією правил цін. Планувальник зовні запускає job повторно; API сам її не запускає.
- Сервіс дедуплікує відповідь за trimmed `CounterpartyId`, оновлює лише змінені записи, додає нових, відновлює soft-deleted і видаляє відсутніх після непорожньої відповіді. Лічильники `Imported/Updated/Restored/Deleted/Skipped/Unchanged` логуються; незмінений запис не отримує нового `UpdatedAt`.
- **До цієї реалізації:** набір `activeCounterpartyIds` поповнювався лише після нормалізації та Identity provisioning. Через це клієнт, **присутній** у відповіді 1C, міг потрапити до видалення після помилки рядка або Identity. Поточний код формує source ID set до цих кроків.
- `Orders` і `OrderDrafts` мають обов'язкові FK до `Counterparties` з `Restrict`; `OrderIdempotencyRecords` та `OrderSyncRetryAudits` також обмежують видалення Order. **До цієї реалізації** звичайний `RemoveRange(staleCounterparties)` не видаляв клієнтів із такими залежностями; поточний `StaleCounterpartyRemoval` видаляє залежності в контрольованому порядку в транзакції.
- За підтвердженим контрактом 1C `GetCounterpartiesAsync()` повертає повний список контрагентів. SOAP-відповідь локально все одно має лише масив без page/total/completion marker; повнота — зовнішнє підтвердження семантики операції. Порожній масив лишається fail-safe і не запускає видалення.
- Source IDs збираються до валідації та provisioning Identity. Наявний у snapshot рядок зі зіпсованим email/price type або помилкою Identity позначається `Skipped`, але його клієнт не потрапляє до списку видалення. Відсутній/порожній ID у будь-якому рядку вимикає видалення за цим проходом.
- `StaleCounterpartyRemoval` видаляє залежності Sales в одній транзакції й записує pending Identity deletion у Sales outbox. Збій Identity не губить `IdentityUserId`; черга повторно обробляється на початку й наприкінці наступного проходу. Транзакції між Sales та Identity немає.
- Додано `CounterpartyReconciliationPostgresTests` для видалення клієнта з Order/Draft, повторного запуску й retry Identity. На цій машині Docker недоступний, тому тест скомпілювався, але був пропущений; PostgreSQL cascade/FK rollback і конкурентний `sync-one-c-orders` ще не підтверджені виконаним тестом.

## Правила продукту

- **R-001.** Сталий ключ звіряння — нормалізований `CounterpartyId` 1C. У межах одного успішного повного проходу кожен ключ відповідає не більше одному локальному клієнту й одному пов'язаному Identity user. Повторний прохід із тими самими даними не створює нового клієнта, користувача або дубляжу правил цін.
- **R-002.** Зміна назви, контактів і типу ціни клієнта в 1C оновлює той самий локальний запис. Некоректний рядок 1C чи невдала підготовка Identity фіксується як `Skipped`, але **наявність його ID у джерелі не є підставою для видалення** локального клієнта.
- **R-003.** Після доведеного повного snapshot клієнт, чий ID справді відсутній у 1C, фізично видаляється із Sales разом із його локальними замовленнями, чернетками, позиціями, правилами цін, записами idempotency, sync/retry станом та іншими залежностями, які виявить FK-аудит. Локальне видалення виконується одразу, навіть якщо замовлення перебуває у стані відправлення до 1C або вже відправлене. Пов'язаний Identity user також підлягає видаленню без зачеплення користувача, прив'язаного до іншого клієнта.
- **R-004.** Підтверджений контракт `GetCounterpartiesAsync()` повертає повний список без paging. Порожній список, транспортний збій або рядок без валідного ID не запускають масове видалення; непорожній snapshot звіряється за множиною всіх trimmed джерельних ID, сформованою до перевірки інших полів. Поріг скорочення та операторський gate з EN-001 більше не є передумовами цього сценарію.
- **R-005.** Видалення в Sales і створення pending Identity deletion запису атомарні в одній Sales транзакції на вибраний набір клієнтів. Повтор Identity deletion ідемпотентний; невдалий запис лишається в черзі з часом останньої спроби та безпечним описом помилки. Повторний прохід не видаляє Identity user, якщо він пов'язаний з іншим чинним Counterparty. PostgreSQL rollback/concurrency поведінка потребує окремого integration доказу.
- **R-006.** Підсумок проходу фіксує imported, updated, unchanged, deleted, skipped і причини пропусків/зупинки видалення без персональних даних чи SOAP body. Той самий snapshot двічі дає нуль нових/видалених на другому проході.

## Сценарії приймання

| ID | Given / When / Then |
| --- | --- |
| **SC-001** | Given повний список 1C містить нового й наявного клієнта; When job запускається двічі з тим самим списком; Then новий створений лише раз, наявний оновлений за ID, дублів немає. |
| **SC-002** | Given 1C змінила назву/ціну клієнта; When повний імпорт завершується; Then той самий ID має актуальні поля, а старих дублів не з'явилось. |
| **SC-003** | Given ID клієнта присутній у 1C, але його email невалідний або Identity provisioning не вдався; When job обробляє snapshot; Then клієнт не видаляється, `Skipped` і причина видимі оператору. |
| **SC-004** | Given повний перевірений snapshot більше не містить клієнта з локальними Order/Draft, включно із замовленням, що відправляється або вже відправлене в 1C; When безпечна фаза звіряння виконує видалення; Then клієнт і локальні залежності фізично відсутні одразу, інші клієнти й записи не змінені, а worker не відновлює видалене замовлення. |
| **SC-005** | Given 1C повернула порожній snapshot, рядок без ID або сталася transport failure; When job запускається; Then локальні клієнти, замовлення та чернетки не видаляються, а неповнота ID фіксується в логах. Поріг скорочення не застосовується, бо операція за контрактом повертає повний список. |
| **SC-006** | Given Sales видалення завершилось, але Identity delete не вдалось; When job запускається повторно; Then pending запис зберігає user/counterparty IDs, повторює видалення і прибирається лише після успіху або підтвердження, що user уже пов'язаний із чинним клієнтом. |

## Дизайн і дані

Власник даних — `OneCCounterpartiesSyncService`; Identity лишається власником облікового запису. За підтвердженим контрактом джерела SOAP operation повертає всі контрагенти одним масивом. Сервіс створює `sourceCounterpartyIds` із непорожніх trimmed ID до нормалізації, provisioning і перевірок Identity. Якщо хоч один ID порожній, deletion set стає порожнім; порожня відповідь також повертається до будь-яких Sales змін. Наявність ID у джерелі, а не успішний імпорт рядка, визначає, чи можна вважати локального клієнта відсутнім.

`StaleCounterpartyRemoval.RemoveAsync` відкриває Sales transaction. Спочатку видаляє `OrderSyncRetryAudits` і `OrderIdempotencyRecords` для клієнтських замовлень; потім видаляє `Orders`, `OrderDrafts`, правила категорій, `Counterparties` і додає `PendingCounterpartyIdentityDeletions`; каскади БД видаляють Order/Draft lines та order sync/draft idempotency. Після збереження transaction commit, далі `DrainIdentityDeletionsAsync` викликає Identity service. Outbox має новий Guid `Id` як первинний ключ і унікальний індекс за парою `CounterpartyId` + `IdentityUserId`. EF migration: `20261022104036_AddPendingCounterpartyIdentityDeletions`.

`sync-one-c-orders` попередньо завантажує due sync rows і Orders, потім оптимістично захоплює рядок через PostgreSQL `xmin` перед зовнішнім send. Видалення не створює повторних Orders: після commit worker оновлює лише sync row, і зниклий рядок не відновлюється. Вже захоплений worker може завершити зовнішній SOAP виклик після локального видалення; його конкурентну взаємодію та rollback наразі не підтверджено спеціалізованим PostgreSQL тестом. Не змінювати `Restrict` на неконтрольований cascade. Видалення замовлення, вже прийнятого 1C, стосується **лише локального** запису; зовнішній документ цим job не відкликається.

| Шлях | Стан | Одна відповідальність |
| --- | --- | --- |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/Services/OneCCounterpartiesSyncService.cs` | реалізовано | source ID set, upsert/restore, fail-closed для empty або неповного ID, виклик видалення |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/SalesOneCReadClient.cs` | наявний | мапінг повного SOAP масиву в Sales DTO; сам snapshot marker не повертає |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/Services/StaleCounterpartyRemoval.cs` | реалізовано | транзакційне видалення Sales залежностей, outbox запис і повтор Identity deletion |
| `src/Sales/Sales.Infrastructure/DataBase/Entities/PendingCounterpartyIdentityDeletion.cs` | реалізовано | збережені ID та стан повтору Identity deletion |
| `src/Sales/Sales.Infrastructure/Configurations/PendingCounterpartyIdentityDeletionConfiguration.cs` | реалізовано | схема outbox таблиці та індекс за Identity user ID |
| `src/Sales/Sales.Infrastructure/Migrations/20261022104036_AddPendingCounterpartyIdentityDeletions.cs` | реалізовано | створення durable outbox таблиці з Guid PK та унікальністю пари CounterpartyId/IdentityUserId |
| `src/Sales/Sales.Infrastructure/Configurations/OrderConfiguration.cs`, `OrderDraftConfiguration.cs`, `OrderSyncRetryAuditConfiguration.cs`, `OrderIdempotencyRecordConfiguration.cs` | перевірено | Restrict/cascade FK, які визначають порядок видалення |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/Services/SyncOneCOrdersService.cs` | наявний | завантаження, optimistic claim через `xmin` і send worker; окремий customer deletion concurrency test відсутній |
| `src/Identity/Identity.Infrastructure/Services/ImportedCounterpartyIdentityProvisioningService.cs` | наявний | ідемпотентне видалення user, якого вже немає |
| `tests/IntegrationTests/Sales/Persistence/CounterpartyReconciliationPostgresTests.cs` | додано, пропущено без Docker | сценарій видалення Order/Draft і retry Identity; результат PostgreSQL ще не отримано |
| `docs/sdd/features/sales/customers.md` | оновлено | фактична карта синхронізації та видалення |

## Задачі, залежності й докази

| ID | Залежить від | Робота | Цільовий доказ/checkpoint |
| --- | --- | --- | --- |
| **EN-001** | — | Повноту `GetCounterpartiesAsync` підтверджено власником інтеграції; повний масив є source contract. Empty/blank ID fail-closed залишаються | контрактне рішення прийнято; operator count gate не потрібен |
| **EN-002** | EN-001 | FK, Order sync і Identity recovery звірено з фактичними конфігураціями та worker; транзакційне видалення + outbox додано | код і міграція є; PG rollback/concurrency ще не підтверджені тестом |
| **TS-001** | EN-001 | Source IDs відокремлені від рядків, що пройшли validation/provisioning; upsert за ID і fail-closed для невалідного source ID | код наявний; спеціалізованих customer sync unit/integration tests не знайдено |
| **TS-002** | EN-002, TS-001 | Sales залежності видаляються в одній transaction; order worker лишає лише зовнішню гонку вже захопленого SOAP request | код наявний; PostgreSQL FK/rollback/concurrency ще не перевірені |
| **TS-003** | TS-002 | Pending Identity deletion зберігається в outbox і повторюється | код і migration наявні; retry сценарій не має спеціалізованого integration доказу |
| **TS-004** | TS-001–TS-003 | Звірити feature-карту й зафіксувати межі доказів | ця зміна оновлює карту та SDD; перевірки нижче не запускалися |

| Правила | Сценарії | Задачі | Рівень перевірки |
| --- | --- | --- | --- |
| R-001–R-002, R-006 | SC-001–SC-003 | TS-001, TS-003 | unit + PostgreSQL integration |
| R-003–R-005 | SC-004–SC-006 | EN-001–EN-002, TS-001–TS-004 | PostgreSQL/1C fake + operational dry run |

## Стан доставки та неперевірені сценарії

Source snapshot contract підтверджено; code paths для source ID reconciliation, Sales transaction delete та pending Identity deletion queue реалізовані. Міграцію `20261022104036_AddPendingCounterpartyIdentityDeletions` додано до коду, але тут її не запускали.

Невирішений delivery evidence: спеціалізовані PostgreSQL integration tests не підтверджують rollback після FK/SaveChanges failure, одночасне видалення пов'язаних Order/Draft/idempotency/retry records, дві одночасні customer sync job і взаємодію deletion із worker, який уже пройшов `TryStartAttemptAsync`. Поточний worker може завершити SOAP send, використовуючи попередньо завантажений Order, після локального deletion commit; код не створює його повторно, але цей сценарій не перевірений PostgreSQL test. Не вважати ці сценарії пройденими до окремої перевірки.
