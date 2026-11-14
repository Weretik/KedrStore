# Platform 005: Minimal API endpoints and ProblemDetails errors

**Status:** accepted 2026-11-14 (owner decisions in the architecture restructure: Minimal API is the last stage, Request ≠ Command, one error mapper with `ProblemDetails`, identical OpenAPI, done together with the frontend). **Owner:** platform; every business module's Api project. **Rules:** [module rules](../../../standards/module-rules.md), [API rules](../../../standards/api-rules.md).

## Goal

Every HTTP endpoint is a Minimal API endpoint grouped by feature, binds its own request type, sends the Application message, and answers through one result mapper. Every error answer is RFC 9457 `ProblemDetails` with a machine-readable `code`. Clients see the same routes, the same OpenAPI document, and the same success bodies; only error bodies change, and only in ways the current clients already read.

**Today (the deviations this removes):** 180 controller actions in 6 Api projects; two result mappers (`BuildingBlocks.Api` `ToActionResult` for Identity, `Ardalis.Result.AspNetCore` for the rest) plus the order workspace's own `WorkspaceErrorAnswers` (`{code, message}`), so a 400 is an Ardalis error array in Identity, `ValidationProblemDetails` elsewhere, and `{code, message}` in the workspace.

**Out of scope:** new endpoints, route or success-body changes, API versioning, typed error schemas in OpenAPI (error responses stay declared by status only, as now), the shared outbox.

## Rules

- R-001 Endpoints live in `<Module>.Api/Endpoints/<Feature>/` (features as in Application): one static `<Feature>Endpoints` class with `Map<Feature>Endpoints(this IEndpointRouteBuilder)` that creates a `MapGroup` per controller route prefix with the same authorization policy and the old controller name as tag, and one `<UseCase>Endpoint` class per use case (its `Map(RouteGroupBuilder)` with name, `Produces`, and its `Handle`), as the [module rules](../../../standards/module-rules.md) require. `<Module>.Api/DependencyInjection/<Module>Module.cs` gets `Map<Module>Endpoints()`, called by the host.
- R-002 Request ≠ Command. A body or query object is a request record in the same feature folder, one file per type; the handler maps it to the Application command or query. Application messages are never bound from HTTP. Existing request and response records keep their names, because the names are OpenAPI schema names (R-007). An optional query value the controller bound as `default` is a nullable parameter with `null` default that the handler turns into `default`, so it stays optional in the document and in behavior.
- R-003 One mapper: `BuildingBlocks.Api/Results/ResultHttp.ToHttpResult()` turns every `Result`/`Result<T>` into an `IResult`. Success keeps today's status and body (200 body, 201 with body, 204 for `Result` without value where the action answered `NoContent` today).
- R-004 Error body: `application/problem+json` with `type`, `title`, `status`, `detail` (the first error message, Ukrainian), `traceId`, and extensions:
  - `code`: the first `ValidationError.ErrorCode` when a rule set its own (a FluentValidation built-in code such as `NotEmptyValidator` does not count), otherwise `validation` (400), `unauthorized` (401), `forbidden` (403), `not_found` (404), `conflict` (409), `unavailable` (503), `unexpected` (500);
  - `errors`: `{ field: [messages] }` for 400 with field errors (the shape of `ValidationProblemDetails`, which Admin, Mobile and Site already read). Keys are the validator's identifiers, unchanged (for example `Request.Password`, `currentPassword`, `file`).
- R-004a Typed error bodies that the OpenAPI document already declares stay as they are: site refresh 502/503 `SiteRefreshResponse` and the public site model 404 redirect address.
- R-005 A 409 that carries a body object today (order checkout `WorkspaceOrderConflict`: `code` + `quote`) keeps those members at the top level as `ProblemDetails` extensions, so `code: quote_changed | idempotency_conflict` and `quote` stay where Admin reads them.
- R-006 Errors that do not come from a result answer `ProblemDetails` too: model-binding failures (400, `code: invalid_request`), 401/403 from authorization (empty body as today, `UseStatusCodePages` adds `ProblemDetails`), unhandled exceptions (500 from `GlobalExceptionHandler`, `code: unexpected`, never the exception text).
- R-007 The generated `/openapi/v1.json` stays identical to the snapshot taken before the move: paths, methods, operation ids, parameters (complex query objects stay expanded through `[AsParameters]`), request and response schemas, tags (the old controller name without `Controller`), status codes. Error responses are `.ProducesProblem(status)`. The comparison ignores only what MVC and Minimal API list differently without a contract meaning: MVC's extra media types (`text/json`, `text/plain`, `application/*+json`), `application/problem+json` against MVC's `application/json` for errors, and the order of the top-level tag list. Clients are not affected: kedr-web generates its types from the hand-written contracts in `docs/sdd/contracts/`.
- R-008a The Work activity log (spec work/001, R-5) becomes `WorkActivityMiddleware` after authorization: it logs `Admin*` endpoint groups by tag and, until the last module moves, `Admin*` controllers. The operation of an endpoint is its endpoint name (`updateAdminMediaFolder`), of a controller its action name.
- R-008 Behavior around the transport stays: routes, `[Authorize]` policies and anonymous endpoints, the cookie CSRF middleware (Minimal API form endpoints call `.DisableAntiforgery()`, because the host's CSRF check already covers them), request size limits on uploads, rate limits, output and response compression, JSON options (one `ConfigureHttpJsonOptions` with the same settings as `AddControllers`).
- R-009 When the last module moves: no `ControllerBase` remains, `AddControllers()`, `Ardalis.Result.AspNetCore`, `ResultToActionResult` and `WorkspaceErrorAnswers` are removed, and architecture tests fail on a controller or an endpoint outside `Endpoints/<Feature>/`.

## Scenarios

- SC-001 A manager saves a product with an empty name: 400 `application/problem+json`, `code` is the validator's error code or `validation`, `errors.Name` holds the Ukrainian message; the admin shows it under the field.
- SC-002 A request for a deleted article: 404 `ProblemDetails` with `code: not_found`; the admin shows «Не знайдено».
- SC-003 The order workspace refuses an unknown product in a quote: 400 with `code: product_not_found`-style workspace code; the admin shows the message for that `reason` (needs F-001).
- SC-004 Checkout with a changed quote: 409 with `code: quote_changed` and `quote` at the top level; the admin opens the «ціни змінилися» dialog as today.
- SC-005 A sign-in with a 600-character password: 400 `ProblemDetails` with `code: validation` and `errors["Request.Password"]` (Identity used an Ardalis array before; both clients read the new shape).
- SC-006 `GET /openapi/v1.json` after each module's move equals the committed snapshot byte for byte after normalization.
- SC-007 An exception in a handler: 500 `ProblemDetails` with `traceId` and `code: unexpected`, no stack trace.
- SC-008 A cookie request without the CSRF header to a moved upload endpoint is still refused by the host middleware.

## Clients (kedr-web)

Checked 2026-11-14 at kedr-web `55330647`:

- Admin `libs/admin/shared/api-client/src/errors/api-error.ts` already reads `ProblemDetails` (`detail`, `title`, `errors`, `traceId`), a top-level `code`, and passes the 409 body as `conflict`. Its `reason` is set only when there are no field errors.
- Mobile `libs/mobile/shared/api-client/src/errors/api-error.ts` reads the same `ProblemDetails` members.
- Site cart `libs/site/cart/data-access/src/lib/contracts/order-response.ts` reads `errors` as a dictionary (Catalog quick order already answers `ValidationProblemDetails`).

- F-001 (kedr-web, before EN-002 ships) Admin `toApiError`: keep `reason: data.code` in the validation branch also when field errors exist, so the workspace messages keyed by `reason` keep working. Mobile: the same, for symmetry. Backward compatible with today's bodies.
- F-002 (kedr-web, after each backend module ships) Sync the OpenAPI snapshot; it must show no change except the `SOURCE` commit.

## Design

```text
src/BuildingBlocks/BuildingBlocks.Api/
├── Results/ResultProblems.cs              existing (EN-002): failed result → ProblemDetails with code/errors/traceId
├── Results/ProblemCodes.cs                existing (EN-002): validation, invalid_request, unauthorized, forbidden, not_found, conflict, unavailable, unexpected
├── Results/ProblemDetailsSetup.cs         existing (EN-002): AddApiProblemDetails, code + traceId for binding, status-page and exception problems
├── Results/ResultToActionResult.cs        existing (EN-002): controllers' ToActionResult / ToProblem; removed in TS-007
├── Results/ResultHttp.cs                  planned (TS-001): Result → IResult for endpoints
└── Errors/GlobalExceptionHandler.cs       existing: code: unexpected through ProblemDetailsSetup
src/<Module>/<Module>.Api/
├── Endpoints/<Feature>/<Feature>Endpoints.cs   planned: MapGroup, policies, tags, Produces, handlers
├── Endpoints/<Feature>/<Operation>Request.cs   planned: HTTP request records (moved from Features/<Feature>/)
├── Endpoints/<Feature>/<Name>Response.cs       existing response records, moved
├── Features/<Feature>/*Controller.cs           removed per module
└── DependencyInjection/<Module>Module.cs       existing: adds Map<Module>Endpoints()
src/Bootstrapper/Host.Api/Program.cs            existing: MapControllers() → Map<Module>Endpoints() for each module
tests/IntegrationTests/Platform/Api/
├── OpenApiSnapshotTests.cs + openapi.v1.json  existing: golden document (EN-001)
└── ProblemDetailsContractTests.cs             existing (EN-002): 400 validation, 400 invalid_request, 401 bodies
tests/UnitTests/BuildingBlocks/Results/ResultToActionResultTests.cs  existing (EN-002): every status → body
tests/ArchitectureTests/Folders/               existing: controller rule becomes an endpoint rule (TS-007)
```

- Status mapping (R-003/R-004): Ok → 200 (no body for a `Result` without value), Created → 201, NoContent → 204, Invalid → 400, NotFound → 404, Conflict → 409, Forbidden → 403, Unauthorized → 401, Unavailable → 503, Error/CriticalError → 500 `unexpected` with our own Ukrainian message (never exception text; `ExceptionBehavior` turns an exception into `Виникла невідома помилка.`). `Ardalis.Result.AspNetCore` answered Error with 422; nothing in the clients reads 422.
- A workspace 400 without its own rule code said `invalid_request` before and says `validation` now; the admin falls back to the message for any code it does not know.
- Snapshot normalization: the JSON document is re-serialized with sorted object keys and tags in name order, and only the JSON media type of each body is compared (R-007).
- Order of moves, smallest first, one PR each so a regression points at one module: Work (9) → Media (11) → Sales (20) → Identity (23) → Content (54) → Catalog (63).

## Risks

| Risk | Guard |
| --- | --- |
| Minimal API binds complex query objects and `IFormFile` differently from MVC | `[AsParameters]` for query objects, explicit `[FromForm]`; the OpenAPI snapshot shows any parameter change; API integration tests per module |
| Antiforgery is on by default for form endpoints in Minimal API | `.DisableAntiforgery()` on form endpoints; SC-008 test |
| Model-binding failure answered 400 with a body by `[ApiController]`, but with no body by Minimal API | `ProblemDetails` for `BadHttpRequestException` (R-006), contract test |
| JSON options differ between MVC and Minimal API | one shared options setup, snapshot of a success body in each module's API tests |
| Admin loses workspace `reason` when field errors are present | F-001 ships first |

## Tasks

- EN-001 Golden OpenAPI snapshot test (`UPDATE_OPENAPI_SNAPSHOT=1` rewrites it after an intended contract change).
- EN-002 Error-body contract tests (Red for R-004…R-006), then one `ProblemDetails` mapper for every controller (`ToActionResult` from BuildingBlocks.Api in all modules; `WorkspaceErrorAnswers` and `Ardalis.Result.AspNetCore` removed). Depends on F-001. Error bodies switch here, once, for all modules. Hand-written contracts reference [problem.openapi.yaml](../../../contracts/problem.openapi.yaml).
- TS-001 … TS-006 Move Work, Media, Sales, Identity, Content, Catalog to `Endpoints/` (one PR each): snapshot identical, module API tests green, Request records split from commands. TS-001 (Work) also turns the activity filter into `WorkActivityMiddleware` and adds the architecture test for `Endpoints.<Feature>`. TS-002 (Media) adds the upload body limit as endpoint metadata, `.DisableAntiforgery()`, and the SC-008 CSRF and missing-file tests. A 5xx that the controller declared without a body stays `.Produces(status)`, as MVC added `ProblemDetails` only to 4xx. TS-003 (Sales) binds `[AsParameters]` query records with nullable properties (defaults applied in the handler, so the schema keeps the MVC shape), turns the optional `Idempotency-Key` and `If-Match` headers into nullable parameters that become empty values, and adds a validator rule for a missing `If-Match`, which MVC had rejected as an implicitly required parameter. TS-004 (Identity) moves the auth, profile and staff groups; login and refresh keep their rate-limit policies through `RequireRateLimiting`, and the avatar uploads share one `AsAvatarUpload()` convention for the body limit, antiforgery and responses. TS-005 (Content) keeps the public and admin controllers of a feature as two groups in one `<Feature>Endpoints`; the article cover keeps the response type the controller declared (`FileContentResult` as `image/webp`) so the document stays the same, and a missing cover upload gets a Ukrainian validator message. TS-006 (Catalog) binds the list queries through Api parameter records (`[AsParameters]`, nullable values with the Contracts defaults) and maps the two product-list routes to two handlers. This removes three query values from the document that the controller listed but ignored or could not bind (`Lang` and `CategorySlug`, which the route overrode, and `PriceTypeRules`, a list of objects), the `CategoryPriceTypeRule` schema they used, and a `categorySlug` path parameter on `/api/catalog/{lang}/products`, whose route has none; the snapshot drops exactly these entries. Lookups answer 404 with the redirect body through `ToLookupHttpResult`.
- TS-007 Remove MVC: `AddControllers`, `MapControllers`, `ResultToActionResult` (`Ardalis.Result.AspNetCore` already goes in EN-002); architecture tests for R-009; module rules, API rules, `frontend-integration.md` errors section, feature cards updated.

Each task: Red (snapshot or contract test fails on the planned change), Green, then the module's API integration tests and architecture tests.

## Rollout and rollback

No schema change. Each PR deploys on its own; the backend and clients stay compatible at every step because F-001 lands before EN-002 and every move keeps the OpenAPI document. Rollback: revert the PR of the step.
