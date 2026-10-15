# Архітектура тестів — граф завдань

## Завдання

| ID | Відповідальність | Залежить від | Файл |
| --- | --- | --- | --- |
| TS-001 | Реорганізувати тести й виправити відповідальність проєктів | немає | [01-test-structure.md](01-test-structure.md) |
| EN-001 | Додати спільний PostgreSQL Testcontainers fixture | TS-001 | [02-postgres-fixture.md](02-postgres-fixture.md) |
| TS-002 | Довести реальну конкурентну поведінку claim через `xmin` | EN-001 | [03-postgres-concurrency.md](03-postgres-concurrency.md) |
| TS-003 | Забезпечити межі залежностей шарів, модулів і тестових проєктів | TS-001 | [04-architecture-tests.md](04-architecture-tests.md) |
| TS-004 | Додати перевірку перед розгортанням і публікацію покриття тестами | TS-002, TS-003 | [05-ci-and-coverage.md](05-ci-and-coverage.md) |
| TS-005 | Запустити й записати перевірки репозиторію | TS-004 | [06-verification.md](06-verification.md) |

Виконуй завдання за порядком залежностей і [правилами delivery](../../../../standards/delivery-rules.md) проєкту; свідчення перевірки записуй у цій специфікації.
