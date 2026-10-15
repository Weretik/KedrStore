# Фаза 03 — Application

- [x] [T090 — контракти Application](application/03.1-order-contracts.md)
- [x] [T100 — команда створення замовлення менеджера](application/03.2-create-order.md)

## Контрольна точка

Команда перевіряє ввід менеджера, підтверджує контрагента Sales і атомарно фіксує `Order` разом із `OneCOrderSync(Pending)`. Вона не викликає SOAP.
