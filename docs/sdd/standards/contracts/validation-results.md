# Validation, results and errors

FluentValidation validators are registered by Application assembly scanning and run through the Mediator validation behaviour. Put them in the use-case Validators folder.

Validate request shape at the boundary: required input, ranges, formats, enum values, length and input combinations. Validate state/business invariants in Domain or Application use-case logic.

Return Ardalis.Result for expected failures. Preserve the existing API error mapping. Do not throw exceptions for ordinary validation or not-found control flow, and do not reveal internals in responses.
