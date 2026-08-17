# Admin order and 1C synchronization — checklist: specification readiness

- [x] The requirements distinguish the new Sales manager order from the excluded Catalog public quick-order flow.
- [x] Every business, API, and SOAP mapping decision needed for the initial implementation is recorded.
- [x] Each known requirement has a design direction or a documented deferred decision.
- [x] The proposed model contains only data required by the manager order and durable 1C delivery lifecycle.
- [x] Each requirement is covered by concrete task IDs in phases 01–05.
- [x] HTTP route, authorization, idempotency, `Amount` semantics, maximum retry count, and SOAP accepted/error semantics are agreed as the feature baseline.
- [x] The 1C integration boundary, retry rules, time window, logging, and sensitive-data constraints link to durable documentation and the supplied delivery instruction.
