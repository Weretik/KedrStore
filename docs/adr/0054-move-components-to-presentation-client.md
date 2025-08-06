# ADR 0054: Move Web Pages and Components to Presentation.Client Layer

## Status
Accepted

## Date
2025-08-06

## Context
The current KedrStore architecture places App.razor, Routes.razor, and main page components directly in the Presentation layer. This setup limits our ability to use newer Blazor features like RenderMode.Auto, which offers significant advantages for SEO, server load balancing, and dynamic component rendering.

As our application grows, we need to optimize for:
- Better SEO performance
- Reduced server-side rendering load
- More flexible component rendering strategies
- Improved user experience through dynamic rendering decisions

## Decision
We have decided to migrate main project pages, components, App.razor, and Routes.razor to a dedicated **Presentation.Client** layer. This architectural change will:

1. Enable the use of **RenderMode.Auto** for intelligent client/server rendering decisions
2. Improve SEO by ensuring critical content is server-rendered
3. Reduce server load by offloading appropriate components to client-side rendering
4. Create a cleaner separation between presentation concerns

The migration will be done incrementally, starting with the core navigation components and main layout structure.

## Consequences

### Positive
- Improved SEO through better server-side rendering of critical content
- Reduced server load through intelligent rendering mode selection
- Better user experience with faster initial page loads
- More flexible component architecture supporting multiple rendering strategies
- Cleaner separation of presentation layer concerns

### Negative
- Requires careful migration of existing components
- Additional complexity in project structure
- Need for developer training on new rendering modes
- Temporary maintenance overhead during the transition period

## Implementation

1. Create a new **Presentation.Client** project within the solution
2. Migrate App.razor and Routes.razor to the new layer
3. Move core layout components and pages progressively
4. Update component references and dependencies
5. Implement RenderMode.Auto where appropriate
6. Update build and deployment processes
