# ADR 0008: Use Feature-by-Folder Modular Structure

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore aims to support long-term scalability and maintainability by enforcing separation of concerns and minimizing coupling between features.

Instead of organizing code by technical type (e.g., all controllers in one folder, all DTOs in another), the project will follow the **feature-by-folder** layout, where each domain or functional module has its own isolated structure across all layers.

This supports:
- Modularity and autonomy of features
- Improved navigation and onboarding
- Easier versioning and testing per module
- Compatibility with Clean Architecture

## Decision
We will organize the solution by **feature/module**, such as:
- `Products`, `Orders`, `Cart`, `CRM`, `Users`, etc.

Each module will follow this structure:

```
/src
├── Domain/
│   └── Products/
├── Application/
│   └── Products/
├── Infrastructure/
│   └── Products/
├── Presentation/
│   └── Products/
```

Each feature folder will include:
- Domain entities and interfaces
- Application use cases, DTOs, handlers
- Infrastructure implementations
- UI components and pages

## Consequences

### Positive
- Modular code structure
- Better isolation of logic and dependencies
- Facilitates testing and CI/CD per feature
- Easier to onboard new team members to a specific domain

### Negative
- Requires discipline to maintain consistent structure
- May introduce some duplication in folder hierarchy
