# ADR 0010: Use MudBlazor for Admin UI (BackOffice)

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore includes an internal admin panel (BackOffice) for managing products, orders, clients, and settings.  
The admin UI must prioritize productivity, form controls, data grids, dialogs, and navigation components.

While the customer-facing UI is built with Tailwind CSS and Shuffle-generated pages, the admin panel requires a full-featured component library.

**MudBlazor** was selected due to:

- Rich set of Material-based components (tables, dialogs, forms, etc.)
- Full support for Blazor Server and Blazor WebAssembly
- Active development and excellent documentation
- No need for JavaScript interop — pure .NET

## Decision
KedrStore will use **MudBlazor** exclusively for the **Admin Area UI**.

The admin panel will be structured as a separate folder/module within the `Presentation` layer, using:
- `MudTable` for entity lists
- `MudDialog` for editing
- `MudForm` and `MudTextField` for input forms
- `MudTabs`, `MudSelect`, `MudSnackbar`, etc. as needed

## Consequences

### Positive
- Accelerates development of admin features
- Consistent UI/UX with responsive layout
- Strong support for forms, validation, and feedback
- Built entirely in Blazor — no JS dependencies

### Negative
- Material Design style may differ from public site (needs separate layout)
- Additional package dependency (`MudBlazor`)
