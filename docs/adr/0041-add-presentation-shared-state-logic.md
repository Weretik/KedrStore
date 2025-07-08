# ADR 0041: Add Presentation.Shared and State Logic

## Status
Accepted

## Context

To improve modularity and maintainability, the `Presentation.Shared` project was introduced. This project contains shared logic for the presentation layer, including state management components such as `IState` and `StateContainer`.

Previously, state-related logic was scattered across different parts of the presentation layer, making it harder to manage and reuse.

## Decision

The `Presentation.Shared` project was created to centralize shared logic for the presentation layer. The following components were added:

- **IState**: An interface for state management.
- **StateContainer**: A class implementing state management functionality.

These components are now used across the presentation layer to manage state in a consistent and reusable manner.

## Consequences

- **Positive**:
  - Improved modularity and maintainability.
  - Centralized state management logic.
  - Easier reuse of state-related components.

- **Negative**:
  - Initial effort required to refactor and move state-related logic to `Presentation.Shared`.

This change aligns with the project's architecture guidelines and supports future scalability.
