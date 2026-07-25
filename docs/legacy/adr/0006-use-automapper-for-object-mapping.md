# ADR 0006: Use AutoMapper for Object Mapping

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore uses DTOs to transfer data between layers (Application ↔ Domain ↔ UI).  
Manual mapping leads to boilerplate code, especially when DTOs and domain entities have many overlapping fields.

To streamline and centralize mapping logic, a mapping library is needed.

**AutoMapper** was selected due to:

- Declarative mapping profiles that reduce repetitive code
- Convention-based configuration for common mappings
- Integration with DI container
- Wide adoption and good documentation

## Decision
We will use **AutoMapper** for mapping between:
- DTOs and Domain Entities
- Domain Entities and ViewModels (for UI)
- Command DTOs and Entities (in command handlers)

Mapping profiles will be organized per feature/module (e.g., `ProductMappingProfile`, `OrderMappingProfile`) and registered via DI.

## Consequences

### Positive
- Reduces boilerplate mapping code
- Maintains consistency across features
- Easy to extend and test
- Declarative style improves readability

### Negative
- Adds a layer of abstraction that may hide mapping logic
- Can cause runtime errors if mappings are not configured properly
