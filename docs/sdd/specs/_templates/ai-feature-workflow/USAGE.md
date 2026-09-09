# Як дати AI задачу на реалізацію feature

Цей файл призначений для користувача. Він містить готові запити для старту або
продовження реалізації прийнятої SDD-специфікації.

## Що потрібно вказати

У запиті завжди зазначайте:

- точний шлях до feature;
- дозволений scope: уся feature, фаза, сценарії `SC-*` або задачі `TS-*`/`EN-*`;
- додаткове обмеження, якщо частину scope не можна змінювати.

Окремо давати команду для кожного task-файлу не потрібно. У межах дозволеного
scope AI сам переходить до наступної готової задачі за `Depends on` і
`traceability.md`.

## Реалізувати всю feature

```text
Працюй за `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: уся прийнята feature.

Виконуй усі готові задачі за залежностями до delivery checkpoint.
Не зупиняйся лише через перехід до іншого task-файлу або шару.
```

## Реалізувати одну фазу

```text
Працюй за `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: фаза `<00 | 01 | 02 | 03 | 04 | 05>`.

Виконай усі готові задачі цієї фази та потрібні перевірки.
Не переходь до задач поза цим scope.
```

## Реалізувати вибрані сценарії

```text
Працюй за `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: `SC-001`, `SC-002` та їхні обов'язкові `EN-*` залежності.

Виконай усі технічні задачі, потрібні для цих сценаріїв, і запиши evidence у
task-файлах та `traceability.md`. Не реалізовуй інші сценарії.
```

## Виконати конкретні задачі

```text
Працюй за `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: `TS-004`, `TS-005`.

Перевір їхні залежності, виконай задачі та checkpoint-и. Не розширюй scope.
```

## Продовжити перервану роботу

```text
Продовжуй роботу за `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: <попередній scope або нове точне обмеження>.

Перевір поточні task markers, `traceability.md`, git diff і результати
verification. Продовжуй із першої готової незавершеної задачі. Не повторюй
виконану роботу та не відкидай сторонні зміни робочого дерева.
```

## Які файли вказувати

Обов'язково вкажіть:

- каталог feature у `docs/sdd/specs/<module>/...`;
- workflow `docs/sdd/specs/_templates/ai-feature-workflow/`.

Якщо задача вузька, корисно додати шлях до конкретного task-файлу. Додавати
окремо requirements, design, contracts і standards не потрібно: workflow
зобов'язує AI знайти та прочитати релевантні документи.

AI зупиняється після завершення дозволеного scope або через конкретний blocker:
невідоме бізнес-рішення, незакриту залежність, потрібний зовнішній доступ чи
іншу дію, для якої немає дозволу.
