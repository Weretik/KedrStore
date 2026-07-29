# ADR 0009: Use Blazor Web App as the Presentation Layer

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore requires a modern UI platform that supports SEO, interactivity, and modular composition.  
The application must serve both customer-facing pages and admin/internal tools, with flexibility in rendering modes.

Blazor Web App (new in .NET 8) was selected as the Presentation layer because:

- Supports **interactive rendering** with server-side or WebAssembly
- Enables **Razor-based components** with strong .NET tooling
- Provides **full control over routing, layout, and composition**
- Aligns with Clean Architecture principles (no dependency on UI in Domain/Application)

## Decision
We will use **Blazor Web App (ASP.NET Core)** as the UI layer for KedrStore.  
Key rendering strategies:
- **SSR** (static pages, SEO-critical areas)
- **Server interactivity** for customer interaction
- **Blazor Server** for Admin (BackOffice) tools

The project will define a shared layout system and modularize components by feature (`Products`, `Orders`, `CRM`, etc).

## Consequences

### Positive
- SEO-compatible and SPA-capable hybrid rendering
- No JavaScript required — full C# stack
- Component reuse across UI and admin areas
- Tight integration with backend logic and DI

### Negative
- Higher memory footprint (especially with Blazor Server)
- WebAssembly has cold start and limitations in mobile
- Not all third-party JS-based UI libraries are usable
