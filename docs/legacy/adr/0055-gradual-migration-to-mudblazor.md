# ADR 0055: Gradual Migration from Tailwind to MudBlazor

## Status
Accepted

## Date
2025-08-06

## Context
Currently, KedrStore uses two different UI frameworks:
- **TailwindCSS** with Shuffle-generated templates for the customer-facing storefront
- **MudBlazor** for the admin panel (BackOffice)

This dual approach creates several challenges:
- Inconsistent component behavior across the application
- Duplication of styling efforts
- Increased learning curve for new developers
- Maintenance overhead for two separate UI frameworks

While MudBlazor has proven effective for the admin area, we now see potential benefits in standardizing on a single component library across the entire application.

## Decision
We have decided to gradually migrate customer-facing UI components from Tailwind to MudBlazor. This migration will:

1. Start with smaller, isolated components
2. Progress to larger page sections
3. Maintain backward compatibility during transition
4. Preserve SEO and performance benefits

The migration will be incremental, ensuring no disruption to the customer experience.

## Consequences

### Positive
- Unified component library and design language
- Reduced maintenance overhead
- Simplified developer onboarding
- Better component reuse across admin and customer areas
- Consistent behavior for forms, dialogs, and interactive elements

### Negative
- Short-term development overhead during migration
- Need to carefully maintain visual consistency with existing design
- Potential performance considerations with component-heavy pages
- Learning curve for developers primarily familiar with Tailwind

## Implementation

1. Begin with isolated UI components (buttons, inputs, cards)
2. Create MudBlazor equivalents for existing Tailwind components
3. Implement side-by-side testing to ensure visual consistency
4. Progressively replace Tailwind components with MudBlazor versions
5. Update documentation and component guidelines
6. Provide training sessions for team members
