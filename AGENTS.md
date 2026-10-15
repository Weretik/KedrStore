# AI-навігація: KedrStore

Це коротка точка входу для роботи в backend-репозиторії. Фактичні правила, архітектуру, інструменти й команди визначають його код і документація. Читай джерела, потрібні для конкретного завдання. Цей файл діє в усьому репозиторії, якщо вкладений `AGENTS.md` не уточнює окрему ділянку.

## Контекст проєкту

- [Архітектура](docs/sdd/architecture/README.md) — модулі, шари та межі залежностей.
- [Стандарти](docs/sdd/standards/README.md) — сталі правила розробки, delivery та перевірок.
- [Специфікації](docs/sdd/specs/README.md) — вимоги, рішення, завдання й докази виконання фіч.
- [Контракти](docs/sdd/contracts/README.md) — публічна API-поведінка та її синхронізація.
- [Операційна документація](docs/sdd/operations/README.md) — запуск, конфігурація, міграції, seed/jobs і діагностика.
- [Глосарій](docs/product/glossary.md) — сталі бізнес-терміни.

Визнач модуль і зачеплені документи за запитом та кодом; не припускай однакової реалізації в іншому backend-проєкті.

## Маршрутизація завдань

| Завдання | Навичка | Джерело правил проєкту |
| --- | --- | --- |
| Нова або змінена фіча: специфікація | `$backend-feature-spec` | [Правила delivery](docs/sdd/standards/delivery-rules.md) |
| Реалізація прийнятої специфікації | `$backend-feature-delivery` | [Правила delivery](docs/sdd/standards/delivery-rules.md) |
| Кілька агентів на прямий запит користувача | `$backend-feature-orchestrator` | [Специфікації та залежності](docs/sdd/specs/README.md) |
| Аудит коду / план системного виправлення | `$backend-code-audit` / `$backend-remediation-plan` | [Архітектура](docs/sdd/architecture/README.md) і [стандарти](docs/sdd/standards/README.md) |
| HTTP/API та зовнішні контракти | `$backend-api-contract-sync` | [Контракти](docs/sdd/contracts/README.md) |
| Схема БД, міграції та seed-дані | `$backend-database-change` | [Правила БД](docs/sdd/standards/database-rules.md) і [операції з даними](docs/sdd/operations/data/migrations-and-seeding.md) |
| Перевірка змін / передача на review | `$backend-change-verification` / `$backend-pr-handoff` | [Правила тестування](docs/sdd/standards/testing-rules.md) |
| Розбіжності між кодом і документацією | `$backend-docs-sync` | [Індекс стандартів](docs/sdd/standards/README.md) |

## Межі роботи й перевірка

Виконуй усі готові завдання в погодженому обсязі фічі, фази, сценарію або завдання без окремої команди для кожного файла; зупиняйся на межі обсягу, невирішеному продуктовому рішенні чи конкретній перешкоді. Детальний порядок — у [правилах delivery](docs/sdd/standards/delivery-rules.md). Обирай перевірки за [правилами тестування](docs/sdd/standards/testing-rules.md) і конфігурацією зачеплених проєктів. Для нових ідентифікаторів читай [стратегію ID](docs/sdd/standards/identifier-strategy.md).

Запуск `Host.Api` і робота з БД можуть змінювати стан; перед ними звіряй [проєктний порядок міграцій і seed](docs/sdd/operations/data/migrations-and-seeding.md) та дозволене середовище.

Коміти, push, створення гілки чи PR та інші дії зі станом remote виконуй лише за прямим дорученням користувача. Зберігай сторонні зміни робочого дерева. Усі посилання тут ведуть лише до файлів цього репозиторію; назви навичок доступні глобально за іменем; локальних копій навичок у репозиторії немає.
