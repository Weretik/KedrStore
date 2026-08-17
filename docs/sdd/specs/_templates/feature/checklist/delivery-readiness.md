# <feature name> — checklist: readiness for delivery

- [ ] Domain invariants and type boundaries are tested.
- [ ] Persistence constraints, migration and rollback are checked if applicable.
- [ ] The API conforms to the OpenAPI contract from `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, including errors and security.
- [ ] restore/build/test performed or failure documented.
- [ ] Documentation, contract in `docs/sdd/contracts/` and code agreed; aggregated `openapi.yaml` refers to the feature contract via `$ref`.
- [ ] All applicable task IDs are closed; blocker has an owner and a next step.
