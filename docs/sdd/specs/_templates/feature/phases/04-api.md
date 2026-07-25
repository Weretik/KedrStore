# Phase 04: API exposure

**Status:** draft  
**Depends on:** phase 03 and accepted API contract  
**Blocks:** manual Swagger verification and frontend handoff

## Outcome

The application use cases are exposed through stable, thin HTTP endpoints.

## Design

- Controller and action:
- Route and HTTP method:
- Request binding: route, query, body:
- ISender request and CancellationToken:
- ProducesResponseType/OpenAPI:
- Result-to-HTTP mapping:
- Authorization policy or AllowAnonymous:
- Access-control design (when applicable): follow the [access-control checklist](../../../../architecture/security/planning/access-control-checklist.md); do not create users or roles in a non-Identity controller.
- Idempotency-Key requirement for writes:
- Compatibility/additive or breaking change plan:
- Host composition impact:
  - Existing API module: confirm its API assembly marker is already registered as an MVC application part.
  - New API module: add the API project reference to Host.Api, create <Module>ApiAssemblyMarker and add it to ControllersRegistrationExtensions.AddModuleControllers.
  - New Application module: confirm phase 03 added its application marker to Mediator and FluentValidation scanning.
  - New Infrastructure module/service: register it through the existing module/host registration extension called by Program via AddHostServices; do not place feature registration directly in Program unless Program is the established composition point.

## File plan

~~~text
<Module>.Api/
└── Controllers/<Area>Controller.cs

Host.Api/
└── DependencyInjection/ServiceRegistration/
    ├── Web/ControllersRegistrationExtensions.cs     new API module application part
    ├── Pipeline/MediatorRegistrationExtensions.cs   new Application module marker
    ├── Pipeline/FluentValidationRegistrationExtensions.cs
    └── module registration extension               new infrastructure/module service
~~~

## Acceptance criteria

- [ ] Controller has no business rules, EF access or direct adapter call.
- [ ] Public route and response fields match the accepted contract.
- [ ] Authorization is explicit and uses existing policy/role conventions.
- [ ] New registration, user-management, role or policy work follows the Identity ownership and security decisions in the accepted specification.
- [ ] OpenAPI contains the endpoint and expected response metadata.
- [ ] Host.Api composes every new API/Application/Infrastructure assembly through the existing AddHostServices registration flow.

## Verification

- API integration tests:
- OpenAPI contract check:
- Risks:
