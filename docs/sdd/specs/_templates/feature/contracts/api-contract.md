# API change: <feature name>

**Module:** <module>  
**Status:** draft  
**Contract status:** proposed | accepted | implemented

## Goal

Describe the client-visible API change before implementation.

## Route and method

~~~http
<HTTP method> /api/<path>
~~~

## Request

- Route parameters:
- Query parameters and defaults:
- Body:
- Validation and expected 400 errors:

## Response

- Success status and DTO:
- Not found/conflict/invalid statuses:
- Pagination, sorting and filter semantics:
- Compatibility with existing consumers:

## Security and side effects

- Authentication/policy/AllowAnonymous:
- Idempotency:
- Rate limit, audit or logging impact:

## Layer plan

- API controller:
- Application command/query and validator:
- Domain:
- Infrastructure/persistence:

## Acceptance criteria

- [ ]

## Verification

- OpenAPI:
- Integration:
- Manual request:
