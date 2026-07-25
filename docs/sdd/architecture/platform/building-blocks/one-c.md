# BuildingBlocks.Integrations.OneC

## Purpose

This project centralises low-level SOAP mechanics shared by 1C integrations.

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

- OneCSoapClientFactory is registered as a singleton through AddOneCIntegrationServices.
- BasicAuthEndpointBehavior attaches basic authentication to the generated SOAP endpoint.
- Generated Reference.cs is generated code: do not manually refactor it.
- Module-specific 1C clients and mapping remain in that module Infrastructure layer; this project only supplies the shared transport foundation.
