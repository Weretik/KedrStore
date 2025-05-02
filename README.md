# KedrStore — Clean Architecture E-Commerce Platform

KedrStore is a real-world wholesale and retail e-commerce application developed for the Kedr company.  
It follows the **Clean Architecture** pattern and is built using ASP.NET Core MVC, Razor Views, MediatR, AutoMapper, PostgreSQL, and Redis.

---

## 🔧 Technologies & Stack

- ASP.NET Core MVC 8.0
- Razor Pages & Areas (Admin/User)
- Entity Framework Core + PostgreSQL
- MediatR (CQRS)
- AutoMapper
- FluentValidation
- Identity (with custom ApplicationUser)
- Serilog (structured logging)
- Redis (for caching and session)
- Docker-ready configuration

---

## 🏗️ Architecture Overview

```
/Web            - UI layer (Razor, Controllers, ViewModels)
/Application    - UseCases, DTOs, Validators, CQRS
/Domain         - Core business entities and interfaces
/Infrastructure - EF Core DbContext, Repositories, Services
```

- Strict separation of concerns
- One-way dependencies: UI → Application → Domain → Infrastructure
- Supports both synchronous Razor UI and future asynchronous APIs

---

## 📦 Features (current & planned)

✅ Catalog of products with category structure  
✅ Filtering, pagination, sorting  
✅ Basket (cart) functionality  
✅ Contact page and order request form  
✅ Admin panel (products, orders, users)  
⏳ Mobile API (planned, for .NET MAUI)

---

## 🔒 License

This source code is proprietary and provided for demonstration purposes only.  
Any copying, use, or redistribution without explicit permission is prohibited.

© 2025 Vitalii Tsiupin / Kedr