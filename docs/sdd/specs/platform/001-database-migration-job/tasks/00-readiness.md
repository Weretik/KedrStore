# Фаза 00 — Готовність і production-baseline

- [x] T001 Експортувати й записати production `__EFMigrationsHistory` для Catalog, Identity і Sales без чутливих значень. **Залежності:** немає.
- [x] T002 Звірити production-таблиці Identity з історичними міграціями `20260118230934_InitialAppIdentityDbContext` і `20260507141045_AddRefreshSessionStore`. **Залежності:** T001.
- [ ] T003 Створити й перевірити backup Cloud SQL; записати ID backup і відповідального за відновлення без credentials. **Залежності:** T001.
- [ ] T004 Погодити явний порядок migrator contexts і назву/дозволи Cloud Run Job. **Залежності:** T001, T002.

## Контрольна точка

Не відновлюй файли міграцій і не запускай міграцію в production, доки T001–T004 не завершено.

## Запис baseline (2026-07-29)

- Історія продукційного середовища містить `20260118230828_InitialCatalogDbContext`, `20260428115404_Add Translate`, `20260118230934_InitialAppIdentityDbContext` і `20260507141045_AddRefreshSessionStore`.
- Таблиці Identity наявні. `AspNetRoles.Id` має тип `integer`, що підтверджує старий baseline Identity з int-ключами та несумісність із поточною Guid-моделлю.
- Власник погодив руйнівний reset некритичних даних Identity. T003 залишається відкритим до створення й перевірки свіжого backup перед reset; T004 — до перевірки IAM середовища.
