# Phase 04 — API and contract task planning

> Create these tasks only for HTTP or integration behavior.

- [ ] Map every changed operation to its `SC-*` scenarios.
- [ ] Plan the human-readable contract and versioned OpenAPI update before
      transport implementation.
- [ ] Plan focused Red tests before each new transport behavior.
- [ ] Require each endpoint/HTTP behavior task to run its focused API Red test
      before controller or mapping implementation, then Green, refactor,
      regression, and evidence.
- [ ] Create separate tasks for contract, endpoint, HTTP/security mapping, and
      final API acceptance/regression coverage where each is independently reviewable.
- [ ] Give every task a `TS-*` ID, dependencies, exact paths, test level, and checkpoint.
- [ ] Map every API task into `traceability.md`.

## Templates

1. [OpenAPI contract](api/04.NN-contract-documentation.template.md)
2. [Controllers and endpoints](api/04.NN-controllers.template.md)
3. [HTTP behavior, authorization, and Result mapping](api/04.NN-http-behavior.template.md)
4. [API acceptance and regression tests](api/04.NN-tests.template.md)

## Checkpoint

Changed API behavior is contract-first where practical. Each endpoint and HTTP
rule has a planned focused Red test, implementation task, and acceptance or
contract evidence. Features without an HTTP/integration surface use `—` in the
traceability matrix.
