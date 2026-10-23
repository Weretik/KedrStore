# BuildingBlocks.Integrations.OneC

## Purpose

This project centralizes the low-level SOAP mechanism shared by 1C integrations.

## Structure

~~~text
BuildingBlocks.Integrations.OneC/
├── Factory/OneCSoapClientFactory
├── Auth/BasicAuthEndpointBehavior
├── DependencyInjection/AddOneCIntegrationServices
├── Generated/Reference.cs
└── Test/OneCSoapSmokeTest.cs
~~~

## Rules

- `OneCSoapClientFactory` is registered as a singleton through `AddOneCIntegrationServices`.
- `BasicAuthEndpointBehavior` adds basic authentication to the generated SOAP endpoint.
- `Generated/Reference.cs` is generated code; do not refactor it manually.
- 1C clients and module-specific data transformations remain in that module's Infrastructure; this project provides only the shared transport foundation.
