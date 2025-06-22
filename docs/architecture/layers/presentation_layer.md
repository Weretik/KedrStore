# Presentation Layer – KedrStore

## Overview

This document describes the structure and behavior of the **Presentation Layer** in the KedrStore project. KedrStore uses **Blazor Web App** (Server or WebAssembly) for its UI, following Clean Architecture principles. The Presentation Layer interacts with the Application Layer through MediatR and uses structured result handling (`AppResult<T>` and `AppError`) to deliver safe, consistent user experiences.

---

## Key Characteristics

- Based on **Blazor Web App** (.NET 8+)
- UI components are modular and feature-based
- No JavaScript — only Razor, TailwindCSS, and optionally MudBlazor (Admin only)
- Uses `IMediator.Send(...)` to trigger queries/commands
- UI receives `AppResult<T>` and handles loading, errors, and success states
- Supports `.Match(success => ..., error => ...)` result dispatch
- Optional `StateContainer` is used for storing temporary UI state

---

## Folder Structure (Example)

```text
Presentation/
├── Pages/
│   └── Catalog.razor
│   └── ProductDetails.razor
│   └── Error.razor
├── Components/
│   └── ProductCard.razor
│   └── PaginationControls.razor
│   └── AppValidationSummary.razor
├── Shared/
│   └── MainLayout.razor
│   └── AppErrorDisplay.razor
├── State/
│   └── CatalogStateContainer.cs (if used)
├── Program.cs
└── _Imports.razor
```

---

## MediatR Usage in UI

### Example: Triggering a Query from Razor Page

```razor
@inject IMediator Mediator

@code {
    private List<ProductDto> Products;
    private string? ErrorMessage;
    private IDictionary<string, string[]>? ValidationErrors;

    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Send(new GetProductsQuery { ... });

        result.Match(
            success => Products = success,
            error =>
            {
                ErrorMessage = error.Description;
                ValidationErrors = error.ValidationErrors;
            });
    }
}
```

---

## Error Handling in UI

- Uses `<ErrorBoundary>` for unhandled exceptions
- Uses conditional rendering for `AppResult.Failure(AppError)`
- Optional `AppErrorDisplay.razor` component can centralize general errors
- Optional `AppValidationSummary.razor` component can display validation messages

### Example: Inline Validation Display

```razor
@if (ValidationErrors is not null)
{
    <AppValidationSummary Errors="@ValidationErrors" />
}
```

### `AppValidationSummary.razor`
```razor
@if (Errors is not null)
{
    @foreach (var field in Errors)
    {
        <p class="text-sm font-semibold">@field.Key:</p>
        <ul class="ml-4 list-disc text-red-600">
            @foreach (var msg in field.Value)
            {
                <li>@msg</li>
            }
        </ul>
    }
}

@code {
    [Parameter] public IDictionary<string, string[]>? Errors { get; set; }
}
```

---

## State Management (optional)

- Transient UI state is stored in feature-scoped `StateContainer<T>`
- Registered via DI and scoped lifetime
- Allows state persistence across component hops

```csharp
public class CatalogStateContainer
{
    public int CurrentPage { get; set; } = 1;
    public string? SearchTerm { get; set; }
    public Guid? SelectedCategoryId { get; set; }
}
```

---

## TailwindCSS and MudBlazor

- Tailwind is used for base UI styling (configured locally per project)
- Shuffle.dev templates are adapted to Tailwind layout structure
- MudBlazor is only used for admin UI (e.g. tables, forms, dialogs)

---

## Routing and Navigation

- Razor Pages use `@page` directive for routing
- Navigation links use `<NavLink>` or `NavigationManager.NavigateTo()`
- Dynamic routing is supported via `@page "/product/{id}"`

---

## UX: Loading, Empty, Success States

Use patterns like this:

```razor
@if (Products is null)
{
    <p>Loading...</p>
}
else if (!Products.Any())
{
    <p>No products found.</p>
}
else
{
    @foreach (var product in Products)
    {
        <ProductCard Product="product" />
    }
}
```

---

## Summary

The Presentation Layer in KedrStore is structured, modular, and reactive. It delegates all business logic to the Application Layer through MediatR, handles result states explicitly via `AppResult<T>`, and remains UI-focused. Future growth areas include global state handling, toast/snackbar notifications for AppErrors, and broader feature-based Razor module organization.

