# 004 — Робоча область замовлень Admin

- **Модуль:** Sales
- **Споживач:** React Admin
- **Статус:** реалізовано в робочому дереві; потрібна перевірка з PostgreSQL і review
- **Перевірено базис:** 2026-10-21

Окремі операції Admin дають менеджеру пошук і перегляд замовлень, серверний розрахунок вартості та оформлення замовлення з поточного набору товарів. Чинні `/api/admin/orders` і їхні DTO не змінюються в цьому інкременті.

- [Правила й сценарії](requirements/behavior.md)
- [Backend design](design/backend.md)
- [API-рішення](contracts/api-contract.md)
- [Задачі](tasks/README.md)
- [Трасування й готовність](traceability.md)

Модель чернеток визначає [005](../005-admin-order-drafts/README.md). Фактичний стан нинішніх замовлень описано в [Sales feature](../../../features/sales/admin-orders.md).

- [Готовність SDD](checklist/spec-readiness.md)
