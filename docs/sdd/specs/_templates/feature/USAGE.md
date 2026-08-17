# Як створити та реалізувати feature

Надішліть AI це повідомлення для підготовки специфікації:

```text
Працюй за шаблоном `docs/sdd/specs/_templates/feature/`.
Створи специфікацію feature `<NNN>-<feature-slug>` у модулі `<Identity | Catalog | Reference | Accounting | Crm>`.
Мета: `<що має змінитися для користувача або бізнесу>`.
Scope: `<що входить і що явно не входить>`.
```

Приклад:

```text
Працюй за шаблоном `docs/sdd/specs/_templates/feature/`.
Створи специфікацію feature `002-product-archive` у модулі `Catalog`.
Мета: адміністратор може архівувати товар, щоб прибрати його з активного каталогу без втрати історії.
Scope: доменна модель, EF Core migration, CQRS-команда, HTTP endpoint, OpenAPI та тести; UI не входить у scope.
```

AI має скопіювати `feature/` до
`docs/sdd/specs/<module>/<NNN>-<feature-slug>/`, замінити всі плейсхолдери,
заповнити вимоги й дизайн та закрити `checklist/spec-readiness.md` **до**
реалізації коду. Головні фази `00–05` є лише orchestration. AI має створити
тільки потрібні окремі підфази: `00.N` у `tasks/readiness/`, `01.N` у
`tasks/domain/`, `02.N` у `tasks/infrastructure/`, `03.N` у
`tasks/application/`, `04.N` у `tasks/api/` та `05.N` у `tasks/verification/`.
Не додавайте всю реалізацію в головний файл фази. Для HTTP або integration API
виконуйте підфази `04.N` у порядку: controllers → HTTP behavior → OpenAPI →
API tests.

Якщо feature має HTTP endpoint або integration API, ці фази є обов'язковими:
controllers → HTTP contracts/authorization/result mapping → OpenAPI → API tests →
Verification. AI не має пропускати потрібні підфази 04.NN.

AI має працювати за правилом CLI-first: для migration, SQL scripts, OpenAPI
validation/generation, restore, build, tests та інших автоматизованих операцій
спочатку знайти й виконати наявну команду або script проєкту. Ручне створення чи
редагування допускається лише коли generator не може коректно виразити потрібну
зміну, і причина має бути зафіксована в задачі.

Для виконання вже підготовленої feature по одній фазі використовуйте
[AI workflow для feature](../ai-feature-workflow/USAGE.md). Не просіть AI
одночасно створювати специфікацію та реалізовувати всі фази: це ускладнює
перевірку scope, data model і API-контракту.
