# Phase 03 — Application

- [x] [T090 — Application contracts](application/03.1-order-contracts.md)
- [x] [T100 — Create manager order command](application/03.2-create-order.md)

## Checkpoint

The command validates the manager input, confirms the Sales counterparty, and commits `Order` with `OneCOrderSync(Pending)` atomically. It does not invoke SOAP.
