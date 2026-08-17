# Admin order and 1C synchronization — manager order creation

## Behavior

- The frontend sends the selected customer's 1C identifier and the order data to the protected Sales endpoint.
- The application verifies that the counterparty exists in Sales and is not soft-deleted before an order can be created.
- The system assigns its own immutable order identifier. The manager does not supply the identifier used as the 1C external key.
- The command stores the order, order lines, and one `OneCOrderSync` record in `Pending` state atomically. The HTTP request does not wait for a 1C call.

## Rules and invariants

- Every order belongs to exactly one existing Sales counterparty.
- The order keeps only the counterparty identifier. Customer name, phone, and email remain owned by the current Sales `Counterparty` record and are read when needed.
- An order contains at least one line; each line has a valid catalog/1C product identifier and a strictly positive integer quantity.
- Each line contains `Amount`: the total amount for the whole requested quantity, not the unit price.
- `OrderId` is stable for the full order lifetime and is used unchanged in every SOAP retry.
- A failed local transaction creates neither a persisted order nor a synchronization record.

## Acceptance scenarios

1. Given an active counterparty and a valid non-empty order, when an authorized manager creates it, then Sales persists the order and a `Pending` synchronization record in one transaction and returns the local order identifier.
2. Given an unknown or soft-deleted counterparty, when a manager creates an order, then the request fails and no order or synchronization record is stored.
3. Given invalid lines, when a manager submits the request, then validation fails before persistence or a SOAP call.
4. Given a successfully created order, when the create request finishes, then no synchronous SOAP call has been required for the successful HTTP response.
