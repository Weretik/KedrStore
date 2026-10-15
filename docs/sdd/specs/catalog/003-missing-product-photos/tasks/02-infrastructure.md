# Етап 02 — Планування задач Infrastructure

- [x] Виконати [EN-001 — міграція збереження](infrastructure/02.1-persistence-migration.md).
- [x] Виконати [TS-002 — цілісність збереження](infrastructure/02.2-persistence-integrity.md).
- [x] Виконати [TS-003 — перевірка CDN](infrastructure/02.3-cdn-probe.md).

## Контрольна точка

PostgreSQL володіє погодженим зв'язком та індексом, а HTTP-адаптер
класифікує відповіді CDN, не завантажуючи повні зображення й не виходячи
за межі дозволеного хоста.
