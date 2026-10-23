# Feature Access-Control Checklist

Apply this checklist in the relevant [feature specification](../../../specs/README.md).

## Decision sequence

```text
Does an existing policy cover the operation?
  | yes -> protect the endpoint with it; do not add a role or a new user flow
  |
  no
  v
Is this a new business permission or a new user type?
  | permission -> add a named policy and map it to existing roles
  | user type -> add a role, deliberately provision/assign it, then add a policy if needed
  |
  v
Can an anonymous caller create this user?
  | no  -> Admin/invitation/import feature
  | yes -> registration feature with confirmation and abuse protection
```

## Specification checklist

- [ ] Name the protected operation in business terms.
- [ ] Use an existing policy or explain why a new one is needed.
- [ ] For a new policy: name it in `PolicyNames` and define role requirements in Host.Api.
- [ ] For a new role: add a constant, plan initial provisioning and deployment, and define assignment rules; do not leave a declared but unavailable role.
- [ ] State who calls the endpoint and deliberately mark anonymous endpoints with `AllowAnonymous`.
- [ ] If users are created or roles assigned, define the feature as an Identity module use case.
- [ ] Decide whether existing refresh sessions are revoked after a role, password, or account-status change.
- [ ] Add rate limits and safe errors for anonymous endpoints or endpoints with credentials.
- [ ] Include `401 Unauthorized`, `403 Forbidden`, and success cases in the API contract and Swagger verification plan.

## Manual verification matrix

| Case | Expected result |
| --- | --- |
| No bearer token on a protected route | `401` |
| Authenticated user without the required role | `403` |
| Required role and valid request | Successful response as defined by the contract |
| Explicitly anonymous endpoint | Works without a bearer token only when its validation permits it |
| Refresh without CSRF or with a mismatched value | `400` |

After changing roles or authorization rules, verify a new login. Tokens contain role claims obtained when issued, so check sessions created before the change separately if deployment requires immediate effect.
