# Авторизація

## Типове правило

`Host.Api` налаштовує резервне правило авторизації, яке вимагає автентифікованого користувача. Тому кінцева точка захищена, якщо явно не оголошує `[AllowAnonymous]`. Анонімний доступ потрібно явно описати в API-контракті й перевірити в межах фічі.

Автентифікація відповідає на питання **хто викликає**, авторизація — **чи має цей викликач право виконати операцію**.

## Ролі

Константи ролей містяться в `Identity.Domain/Authorization/RoleNames.cs`.

| Роль | Поточний зміст | Створює `Host.Jobs --job=migrate` |
| --- | --- | --- |
| `Admin` | Адміністрування системи | так |
| `Manager` | Керування вмістом | так |
| `User` | Звичайний автентифікований користувач | так |
| `Guest` | Оголошена константа ролі | ні |

`RoleSeeder`, який запускає окремий migration job, створює лише Admin, Manager і User. Не призначай `Guest`, доки не визначено його створення та бізнес-зміст.

## Політики доступу

Назви policies містяться в `Identity.Application/Security/Policies/PolicyNames.cs`; вимоги до ролей налаштовує `Host.Api` в `AuthorizationPoliciesRegistrationExtensions`.

| Policy | Дозволені ролі |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin |
| `RequireManagerRole` / `CanManageProducts` / `CanManageOrders` | Manager, Admin |
| `CatalogRead` / `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | Константа є, але policy наразі не зареєстрована |

Не використовуй `RequireTenantAccess` на кінцевій точці, доки немає реєстрації в хості та визначеної семантики.

## Захист кінцевих точок

Віддавай перевагу наявній названій policy, що описує операцію:

```csharp
[HttpPut("{id:guid}")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public async Task<IActionResult> Update(Guid id, ...)
```

Controller лише оголошує доступ. Command/query однаково проходить через `ISender`; бізнес-інваріанти залишаються в Domain/Application. Не замінюй policy перевіркою ролі всередині controller.

Для публічної кінцевої точки використовуй `[AllowAnonymous]` і зафіксуй причину в контракті фічі. Це перевизначає резервне правило авторизації.

## Інфраструктура поточного користувача й дозволів

BuildingBlocks Infrastructure надає абстракції користувача запиту й дозволів, зокрема `HttpContextCurrentUserService` і `ClaimPermissionService`. Якщо сценарій використання потребує даних поточного користувача, використовуй чинну абстракцію Application. Не інжекти `HttpContext`, `UserManager<AppUser>` або `RoleManager<AppRole>` у Domain чи Application іншого модуля.

## Додавання policy або ролі

1. Перевір, чи наявна policy вже описує бізнес-операцію.
2. Якщо ні, визнач назву policy в Identity Application, а вимогу до ролей — у реєстрації авторизації Host.Api.
3. Якщо потрібна нова роль, додай константу в Identity Domain і заплануй її створення окремим migration job. Визнач, хто з наявних і майбутніх користувачів її отримує.
4. Захисти кінцеву точку новим правилом, онови API-контракт і вручну перевір як заборонені, так і дозволені запити.

Додавання ролі змінює авторизацію, а не лише рядок: воно зачіпає початкове створення, призначення користувачам, документацію й порядок розгортання.
