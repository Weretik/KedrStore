# Cross-cutting pipeline

## Mediator

Host.Api scans the Catalog, Identity, Sales and BuildingBlocks Application assemblies. The configured pipeline order is:

1. request logging;
2. exception handling;
3. performance measurement;
4. FluentValidation;
5. domain-event dispatch after the handler succeeds.

All use-case entry points go through Mediator unless an existing module has an explicitly documented exception.

## Validation and errors

Validators live beside their use case in Validators and validate input shape: required values, ranges, enum values, string length and coherent combinations such as PriceFrom <= PriceTo.

Domain code validates business invariants and state transitions. Do not duplicate a rule across layers without a deliberate defensive reason.

ValidationBehavior converts validation failures to Ardalis.Result.Invalid when the response is Result or Result<T>. Controllers preserve the existing result-to-HTTP mapping.

## Domain events

Entities collect events through IHasDomainEvents. The dispatcher behaviour collects, clears and publishes them through Mediator after successful handler execution. Follow the existing collect → dispatch → clear lifecycle; do not publish events directly from controllers.

## Observability and security

Use structured Serilog templates without secrets, tokens, connection strings or sensitive payloads. Preserve correlation and cancellation. Authorization is configured as policies in the host; an anonymous endpoint requires explicit AllowAnonymous because the host has an authenticated fallback policy.
