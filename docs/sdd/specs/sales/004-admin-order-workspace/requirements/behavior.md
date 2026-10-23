# Rules and acceptance scenarios

## Rules

- **R-001.** The list contains completed Sales Orders only, not drafts. The server applies filters and sorting and calculates `totalRecords` before `Skip/Take`; defaults are page 1/20, maximum 100.
- **R-002.** Search uses no external service: trim, normalize whitespace, and use case-insensitive tokens; every token must occur in the order number or customer name, with tokens combined using AND. A numeric query may also match the exact internal `orderId`. `counterpartyId` is a separate exact filter combined with search using AND.
- **R-003.** Allowed sorts: `-createdAt` (default), `createdAt`, `orderNumber`, `-orderNumber`, `customerName`, `-customerName`, `totalAmount`, `-totalAmount`; `orderId` is the final key for stable pagination.
- **R-004.** The list returns compact rows; details return saved lines, amount, customer, and safe 1C status. Reads do not start the integration.
- **R-005.** The server calculates prices using current Catalog prices and pricing rules for the selected Counterparty. The client sends only product and quantity; missing price, nonexistent or unavailable product, inactive customer, zero/negative quantity, or amount overflow produces a validation error. Manual price editing is impossible. The response amount is a preliminary quote; prices are recalculated at checkout.
- **R-006.** Successful checkout atomically creates an Order, lines with a snapshot of server price, order number, and `OneCOrderSync(Pending)`; the existing job processes the order later. The request has an `Idempotency-Key`: the same key and body return the same result; a different body conflicts. On failure, no partial order/sync record exists.
- **R-007.** Workspace operations are anonymous at this stage. Do not log order bodies or customer data.

## Scenarios

- **SC-001:** Given orders from different customers exist; when Admin searches for part of a number or multiple words in a name and selects a customer; then the correct rows, `totalRecords`, and stable pages are shown.
- **SC-002:** Given an order exists; when Admin opens its details; then saved lines, total amount, and 1C status are shown without changing the data; an unknown ID returns 404.
- **SC-003:** Given a customer and products are selected; when Admin requests a quote; then the server returns prices under that customer's rules and explains invalid lines without creating an Order.
- **SC-004:** Given a quote succeeded; when a price changes before checkout; then checkout recalculates and saves the current server amount; UI must show confirmation of the current amount before retrying checkout.
- **SC-005:** Given a valid customer and lines; when Admin checks out with an idempotency key; then one Order with a number and `Pending` sync exists; repeating the request does not duplicate it.
- **SC-006:** Given an invalid product/customer ID or missing price; when Admin checks out; then 400/404 with safe field errors is returned and no Order is written.
