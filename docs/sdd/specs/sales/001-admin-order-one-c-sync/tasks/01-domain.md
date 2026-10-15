# Фаза 01 — Domain

- [x] [T010 — агрегат замовлення та позиції](domain/01.1-order-aggregate.md)
- [x] [T020 — життєвий цикл передавання до 1С](domain/01.2-one-c-order-sync.md)
- [x] [T030 — тести Domain](domain/01.3-order-domain-tests.md)

## Контрольна точка

`Order` і `OneCOrderSync` захищають усі переходи стану без залежностей від SOAP, EF Core або HTTP.
