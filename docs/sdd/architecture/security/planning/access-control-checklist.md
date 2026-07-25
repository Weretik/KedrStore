# Access-control feature checklist

Use this checklist in a feature folder created from [the feature template](../../../specs/_templates/feature/template-feature.md).

## Decision path

```text
Does an existing policy cover the operation?
  | yes -> protect the endpoint with it; do not add a role or user flow
  |
  no
  v
Is this a new business permission, or a new kind of user?
  | permission -> add a named policy and map it to existing roles
  | user kind   -> add role, seed/assign it deliberately, then add policy if needed
  |
  v
Can an anonymous caller create that user?
  | no  -> Admin/invitation/import feature
  | yes -> registration feature with confirmation and abuse controls
```

## Specification checklist

- [ ] Name the protected operation in business terms.
- [ ] Reuse an existing policy, or document why a new policy is required.
- [ ] For a new policy: name it in `PolicyNames` and define its role requirements in Host.Api.
- [ ] For a new role: add its constant, seeding/rollout plan and assignment rules; do not leave a role declared but unavailable.
- [ ] State who may call the endpoint and mark anonymous endpoints with `AllowAnonymous` intentionally.
- [ ] If users are created or assigned roles, make the feature an Identity-module use case.
- [ ] Define whether existing refresh sessions must be revoked after role/password/account-state changes.
- [ ] Add rate limits and safe error responses for anonymous or credential-related endpoints.
- [ ] Include `401 Unauthorized`, `403 Forbidden` and success cases in the API contract and Swagger test plan.

## Manual test matrix

| Case | Expected result |
| --- | --- |
| No bearer token on protected route | `401` |
| Authenticated user without required role | `403` |
| Required role and valid request | Success contract |
| Explicit anonymous endpoint | Works without bearer token only where its validation permits it |
| Refresh without/mismatching CSRF value | `400` |

After changing roles or policies, test a fresh login. Tokens contain role claims at issuance, so test sessions created before the change separately if the rollout requires immediate effect.
