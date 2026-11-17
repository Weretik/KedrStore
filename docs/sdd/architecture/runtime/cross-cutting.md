# Cross-cutting pipeline

## Mediator

`Host.Api` scans the Application assemblies for Catalog, Identity, Sales, and BuildingBlocks. The configured pipeline order is:

1. request logging;
2. exception handling;
3. performance measurement;
4. FluentValidation.

All use-case entry points go through Mediator unless an existing module has an explicitly documented exception.

## Validation and errors

Validators live alongside their use case in `Validators` and check input shape: required values, ranges, enum values, string lengths, and consistent combinations such as `PriceFrom <= PriceTo`.

Domain code checks business invariants and state transitions. Do not duplicate a rule across layers without a deliberate reason for a defensive check.

`ValidationBehavior` converts validation failures to `Ardalis.Result.Invalid` when the response is `Result` or `Result<T>`. Endpoints answer it through `ToHttpResult` as a 400 `ProblemDetails` with `errors`.

## Observability and security

Use structured Serilog templates without secrets, tokens, connection strings, or sensitive request data. Preserve correlation and cancellation. Host rules configure authorization; an anonymous endpoint requires explicit `AllowAnonymous` because the host has a fallback policy requiring authentication.
