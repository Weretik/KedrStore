# Як створити та реалізувати feature

Надішліть AI це повідомлення для підготовки специфікації:

```text
Працюй за шаблоном `docs/sdd/specs/_templates/feature/`.
Створи специфікацію feature `<NNN>-<feature-slug>` у модулі `<Catalog | Platform | Identity>`.
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
реалізації коду.

Для виконання вже підготовленої feature по одній фазі використовуйте шаблон
`ai-feature-workflow/`. Не просіть AI одночасно створювати специфікацію та
реалізовувати всі фази: це ускладнює перевірку scope, data model і API-контракту.
