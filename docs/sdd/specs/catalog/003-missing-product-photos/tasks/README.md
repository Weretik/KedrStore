# Відсутні фотографії товарів — граф задач

Усі задачі виконано після дозволу на реалізацію. Свідчення записано
в кожному докладному файлі задачі.

```text
TS-001 photo-check domain state
   └── EN-001 migration and backfill
          ├── TS-002 persistence integrity
          ├── TS-004 job/import orchestration ──┐
TS-003 CDN probe adapter ────────────────┘      │
                                                ├── TS-005 unavailable-photo query
                                                │      └── TS-006 HTTP contract/endpoint
                                                └── EN-002 Host.Jobs + Cloud Run wiring
TS-001..TS-006 + EN-001..EN-002 ── TS-007 verification and delivery
```

| ID | Відповідальність | Залежить від | Докладна задача |
| --- | --- | --- | --- |
| TS-001 | Стан перевірки фотографії та інвалідація URL | немає | [Domain-стан](domain/01.1-photo-check-state.md) |
| EN-001 | Відображення EF, згенерована міграція та заповнення активних товарів | TS-001 | [Передумова збереження](infrastructure/02.1-persistence-migration.md) |
| TS-002 | Життєвий цикл PostgreSQL і цілісність застарілого запису | EN-001 | [Поведінка збереження](infrastructure/02.2-persistence-integrity.md) |
| TS-003 | Обмежена HTTP-перевірка CDN | TS-001 | [Адаптер CDN](infrastructure/02.3-cdn-probe.md) |
| TS-004 | Узгодження імпорту й одноразова задача перевірки | TS-001, EN-001, TS-003 | [Оркестрація задачі](application/03.1-check-product-photos.md) |
| TS-005 | Сторінковий сценарій читання недоступних фотографій | EN-001 | [Запит списку](application/03.2-get-unavailable-products.md) |
| TS-006 | OpenAPI й анонімний HTTP endpoint | TS-005 | [API endpoint](api/04.1-missing-photo-endpoint.md) |
| EN-002 | Команда хоста, операційна документація й визначення Cloud Run | TS-004 | [Підключення розгортання](enablers/EN-002-cloud-run-job.md) |
| TS-007 | Регресія, перевірка runtime-контракту й звіт про постачання | усе вище | [Перевірка](verification/05.1-feature-verification.md) |

Реалізація просувається за готовністю залежностей, а не лише за номером етапу.

## Інкремент 2026-10-19

Старі TS-001–TS-007 та їхні докази залишаються історією. Нові [R-011–R-014 і SC-012–SC-015](../requirements/table-browsing.md) виконано в порядку TS-008 (OpenAPI та запит) → TS-009 (фільтрація, підрахунок, валідація і цільові тести) → TS-010 (HTTP-прив'язка, цільовий API-тест і оновлення карти фіч). Свідчення та межі перевірки наведено в [traceability](../traceability.md).

2026-10-19 — TS-011 виправляє переклад SQL-сортування з [R-015/SC-016](../requirements/table-browsing.md). PostgreSQL regression-тест додано; локально він пропущений через недоступний Docker, тому фізичний SQL-запит лишається умовою CI. Вузькі тести з EF InMemory пройшли `8/8`; історичні результати TS-008–TS-010 не змінено.
