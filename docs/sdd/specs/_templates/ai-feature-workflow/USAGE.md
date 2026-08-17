# Як запускати AI для одного файла фази feature

Спочатку створіть і погодьте специфікацію за
[шаблоном feature](../feature/USAGE.md). Потім надсилайте AI одну фазу за раз:

```text
Працюй за регламентом `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Поточний файл фази: `<tasks/00-readiness.md | tasks/application/03.2-create.md | …>`.
```

Приклад:

```text
Працюй за регламентом `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/catalog/002-product-archive`.
Поточний файл фази: `tasks/domain/01.1-aggregate.md`.
```

Головні фази виконуються в такому порядку, але кожна з них створює лише потрібні
окремі підфази. AI виконує одну підфазу за раз:

| Фаза | Результат |
| --- | --- |
| `00` | створює `00.N` підфази для scope, CQRS use cases та tooling |
| `01` | створює `01.N` Domain-підфази |
| `02` | створює `02.N` Infrastructure-підфази |
| `03` | створює `03.N` Application-підфази |
| `04` | створює `04.N` API-підфази для HTTP/integration feature |
| `05` | створює `05.N` Verification-підфази |

Після головної фази назвіть точний створений файл підфази. Наприклад:

```text
Працюй з `tasks/application/03.2-create.md`.
```

Без цієї команди AI не переходить до наступного файла. Якщо залежна фаза або
підфаза має незакриті задачі чи blocker, спершу усуньте їх або явно змініть scope.
