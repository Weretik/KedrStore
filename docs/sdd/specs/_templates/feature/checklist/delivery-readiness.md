# <feature name> — checklist: delivery readiness

- [ ] Every in-scope `SC-*` row in `traceability.md` is verified or explicitly deferred.
- [ ] Domain invariants and type boundaries are tested where applicable.
- [ ] Application orchestration, validation, and result behavior are tested.
- [ ] Persistence constraints, migration, rollback, and integrations are checked where applicable.
- [ ] The API conforms to the agreed OpenAPI contract, including errors and security.
- [ ] Each behavior-implementing `TS-*` task records valid Red, Green, refactor,
      and regression evidence.
- [ ] Every `EN-*` exception records its reason and replacement verification.
- [ ] Restore/build/test ran or the exact failure and ownership are documented.
- [ ] Documentation, contracts, implementation, and test names agree.
- [ ] All applicable task IDs are closed; each remaining blocker has an owner and next step.
