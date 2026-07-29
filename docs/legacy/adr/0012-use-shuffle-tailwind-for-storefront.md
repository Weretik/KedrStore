# ADR 0012: Use Shuffle.dev + TailwindCSS for Storefront UI

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore has two separate UI segments:
- A **customer-facing storefront** (product catalog, cart, search)
- An **internal admin panel** (BackOffice)

For the storefront, design consistency, mobile responsiveness, SEO, and speed of development are key.  
Manual Tailwind layout creation is time-consuming and error-prone for non-designers.

**Shuffle.dev** was selected to generate production-quality Tailwind-based UI templates.  
**TailwindCSS** was selected as the primary utility-first CSS framework.

## Decision
We will use:
- **Shuffle.dev** for generating landing, catalog, product pages, etc.
- **TailwindCSS** as the main styling framework in the customer UI
- No JavaScript frameworks — interactivity handled via Blazor

Shuffle templates will be adapted into Blazor components and pages using:
- Clean semantic HTML
- Razor markup with Tailwind classes
- SEO-friendly layouts and meta handling

Tailwind will be installed locally under the `Presentation/Web` folder via `npm`.

## Consequences

### Positive
- Rapid UI development without custom CSS
- Consistent and responsive design
- Clean separation between layout/design and C# logic
- Highly customizable if needed

### Negative
- Requires basic Tailwind knowledge
- HTML from Shuffle may need adaptation for Razor syntax
- Build process needs Tailwind CLI or bundler integration
