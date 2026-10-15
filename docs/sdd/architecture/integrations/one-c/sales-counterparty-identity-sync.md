# Контрагенти 1C і створення облікових записів Identity

## Призначення та відповідальність

Імпорт Sales перетворює контрагента 1C на два пов'язані локальні записи:

```text
1C GetCounterparties
       ↓ SalesOneCReadClient
OneCCounterpartiesSyncService
       ├── Sales.Counterparties (бізнес-клієнт)
       └── IImportedCounterpartyIdentityProvisioningService
                ↓
          Identity.AspNetUsers (обліковий запис для входу, роль User)
```

`Counterparty.Id` у Sales — рядок `CounterpartyId` з 1C після обрізання пробілів. `Counterparty.IdentityUserId` пов'язує його з `AppUser.Id` (GUID). Sales відповідає за запис клієнта; Identity — за користувача для входу, hashing пароля, ролі та параметри облікового запису.

## Методи й поля джерела

`SalesOneCReadClient.GetCounterpartiesAsync()` викликає згенеровану SOAP-операцію `GetCounterpartiesAsync()` і відображає:

| Поле 1C | Локальне використання |
| --- | --- |
| `CounterpartyId` | ID контрагента Sales; вхідне значення початкового пароля імпортованого запису |
| `CounterpartyName` | Назва контрагента та `AppUser.FullName` |
| `Email` | Email Sales, username і email Identity |
| `Phone` | Нормалізований телефон Sales; некоректний необов'язковий телефон ігнорується |
| `DefaultPriceTypeId` | Типова ціна клієнта |

`SalesOneCReadClient.GetCounterpartyCategoryPriceTypesAsync()` викликає `GetCounterpartyCategoryPriceTypesAsync()` і повертає правила `(CounterpartyId, CategoryId, PriceTypeId)`. У `sales-customers-full` синхронізація правил іде після контрагентів, щоб перевірити посилання на них.

## Звіряння контрагентів

`OneCCounterpartiesSyncService`:

1. Якщо 1C не повернула контрагентів, завершується без змін.
2. Прибирає дублікати непорожніх ID, залишаючи останній рядок для кожного.
3. Відхиляє рядок із порожнім ID/назвою, недодатним типовим типом ціни або некоректним/відсутнім email. Логує причину пропуску.
4. Нормалізує email і необов'язковий телефон через `CounterpartyContactNormalizer`.
5. Створює чи знаходить користувача Identity, потім створює, оновлює або відновлює контрагента Sales.
6. Локальних контрагентів, відсутніх у непорожній коректній відповіді джерела, вважає застарілими: видаляє їхні правила цін категорій і записи Sales, потім запитує видалення користувача Identity.

Він також не дозволяє пов'язати один `IdentityUserId` із двома різними контрагентами в одному локальному стані.

## Створення та оновлення користувача Identity

Реалізація — `ImportedCounterpartyIdentityProvisioningService`, зареєстрована в Identity Infrastructure для інтерфейсу Application `IImportedCounterpartyIdentityProvisioningService`.

```text
наявний Counterparty.IdentityUserId → пошук AppUser за ID
                                      ↓ не знайдено
                                  пошук AppUser за email
                                      ↓ не знайдено
                              створити AppUser + роль User
```

Для нового користувача сервіс задає username/email/повне ім'я, `EmailConfirmed` і `LockoutEnabled` з `Identity:ImportedCounterparties`, тоді створює обліковий запис із 1C `CounterpartyId` як вхідним значенням початкового пароля. Для знайденого користувача він оновлює ті самі поля, скидає пароль до поточного `CounterpartyId` і забезпечує роль `User`. Паролі обробляються hashing ASP.NET Identity; застосунок не має логувати або відкривати вхідне значення.

## Безпека й операційні наслідки

- Це не просто імпорт читання: `counterparties` або `sales-customers-full` може створити, оновити, відновити або видалити як дані клієнта, так і облікові записи для входу.
- Збіг email може пов'язати наявного користувача Identity з імпортованим контрагентом. Вважай імпортований email авторитетним лише після перевірки якості даних 1C.
- Зміни Sales зберігаються до видалення застарілих користувачів Identity. Якщо видалення Identity не вдалося, видалення Sales залишається, а в лог пишеться попередження; осиротілий обліковий запис перевір вручну.
- Потік охоплює contexts Sales та Identity й не є однією міжконтекстною транзакцією. Не додавай побічних ефектів, що припускають атомарний rollback без окремого дизайну.
- Правила типів цін категорій пропускаються, якщо контрагента немає в Sales або категорії чи типу ціни немає в Catalog. Якщо довідкові дані змінилися, до `sales-customers-full` синхронізуй Catalog.

## Перевірка після безпечного тестового запуску

1. Використай непродукційного тестового контрагента з коректним email і типовим типом ціни.
2. Запусти `--job=counterparties`; перевір поля контрагента Sales і рівно один пов'язаний `IdentityUserId`.
3. Перевір, що користувач Identity має роль `User` і застосовані параметри імпортованого облікового запису. Не переглядай і не друкуй парольні дані.
4. Запусти `--job=counterparty-category-price-types`; перевір, що залишилися лише правила з коректними локальними посиланнями Catalog.
5. Перевір лічильники imported, updated, restored, deleted і skipped. Досліди пропущені чи видалені записи до повторного запуску.
