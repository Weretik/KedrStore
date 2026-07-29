# Як запускати AI для однієї фази feature

Спочатку створіть і погодьте специфікацію за шаблоном `feature`. Потім
надсилайте AI одну фазу за раз:

```text
Працюй за регламентом `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Поточна фаза: `<00 | 01 | 02 | 03 | 04 | 05>`.
```

Приклад:

```text
Працюй за регламентом `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/catalog/002-product-archive`.
Поточна фаза: `01`.
```

Фази виконуються лише в такому порядку:

| Фаза | Результат |
| --- | --- |
| `00` | scope, data model та OpenAPI/integration contract погоджені |
| `01` | domain model, інваріанти й unit-тести |
| `02` | persistence або integration, migration та integration tests |
| `03` | CQRS request/handler, validation і application tests |
| `04` | API endpoint, authorization, OpenAPI та API tests |
| `05` | restore/build/test, ручні сценарії, documentation і delivery report |

Для наступної фази надсилайте окрему команду:

```text
Переходь до фази 02.
```

Без цієї команди AI не переходить до наступної фази. Якщо попередня фаза має
незакриті задачі або blocker, спершу усуньте їх чи явно змініть scope.
