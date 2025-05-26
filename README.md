# 🛍️ KedrStore — Modular E-commerce Platform

**KedrStore** is a production-level B2B/B2C e-commerce application developed for Kedr.  
It is built using **.NET 8**, **Blazor Web App**, and follows a **Clean Architecture** pattern with layered structure and modular monolith principles.

---

## ⚙️ Tech Stack

- **.NET 8**, **ASP.NET Core**, **Blazor Web App**
- **Entity Framework Core** + **PostgreSQL**
- **MediatR** (CQRS), **AutoMapper**, **FluentValidation**
- **ASP.NET Identity** (custom `AppUser`)
- **Redis** (caching/session), **Serilog**
- **Tailwind CSS**, **DaisyUI**, **MudBlazor**
- **Docker-ready configuration**

---

## 📐 Project Structure

```
/Web            - Razor UI (Pages, Components, Admin Area)
/Application    - UseCases, DTOs, CQRS Handlers, Validation
/Domain         - Core business logic and contracts
/Infrastructure - EF Core setup, Repositories, External services
```

Supports strict separation of concerns and one-way dependencies:  
`UI → Application → Domain → Infrastructure`

---

## 🚀 Features (in progress)

- 🛒 Product catalog with filtering, sorting, pagination  
- 📦 Cart system  
- 🔐 Admin panel (product management, users)  
- 🧾 Contact & order request form  
- 📱 API planned for .NET MAUI (mobile client)

---

## 🧰 Dev Highlights

- Modular monolith structure  
- CQRS with MediatR  
- Razor UI components (no JS)  
- Structured logging with Serilog  
- Integrated Redis for performance  
- SEO-friendly frontend with Tailwind + MudBlazor

---

## 📌 Project Status

✅ Architecture & layering complete  
✅ Catalog UI in progress  
✅ Filtering & pagination done  
🔜 Basket logic  
🔜 Admin CRUD for orders  
🔜 Mobile API (.NET MAUI)

---

## 🔒 License

This source code is proprietary and developed for Kedr.  
Copying or distribution without permission is prohibited.

© 2025 Vitalii Tsiupin / Kedr
