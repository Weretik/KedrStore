# Фаза 02 — Infrastructure

- [x] [T040 — мапінг збереження EF Core](infrastructure/02.1-order-persistence.md)
- [x] [T050 — міграція бази даних Sales](infrastructure/02.2-sales-migration.md)
- [x] [T060 — адаптер запису SOAP до 1С](infrastructure/02.3-one-c-write-adapter.md)
- [x] [T080 — інтеграційні тести Infrastructure](infrastructure/02.5-infrastructure-tests.md)

## Контрольна точка

Схема Sales зберігає один стійкий запис передавання для кожного замовлення, а робота із SOAP залишається на межі Infrastructure. Плановий worker передавання навмисно реалізовано у фазі 05 після завершення HTTP-шляху створення.
