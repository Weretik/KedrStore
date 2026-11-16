# C4 diagrams

**Checked against code:** 2026-11-16 (`src/`, `.github/workflows/deploy-cloudrun.yml`, project references, kedr-web `apps/`).

Three levels of the [C4 model](https://c4model.com): who uses the system and what it talks to (context), which deployable parts it has (containers), and what is inside the API (components). The diagrams are Mermaid and render on GitHub. Why each part looks the way it does is in the [ADRs](../decisions/README.md).

Update a diagram in the same change that adds or removes a module, a host, a deployable app, an external system, or a connection between them.

## Level 1: System context

```mermaid
flowchart TB
    buyer["<b>Buyer</b><br/>[Person]<br/>Wholesale or retail customer of door hardware"]
    manager["<b>Manager</b><br/>[Person]<br/>Takes orders, works with customers"]
    staff["<b>Staff: Admin, SEO</b><br/>[Person]<br/>Runs the catalog, content and settings"]

    kedr["<b>Kedr platform</b><br/>[Software system]<br/>Public catalog site, admin, manager app and backend"]

    onec["<b>1C</b><br/>[External system]<br/>Source of products, prices, stock, customers"]
    cdn["<b>CDN cdn.express</b><br/>[External system]<br/>Stores and serves pictures"]
    telegram["<b>Telegram</b><br/>[External system]<br/>Managers' chat for quick orders and alerts"]
    gcp["<b>Google Cloud</b><br/>[External system]<br/>Cloud Run, Scheduler, Secret Manager,<br/>Cloud Logging and Cloud Trace"]

    buyer -->|"Browses catalog, sends quick orders (HTTPS)"| kedr
    manager -->|"Uses the manager app and admin"| kedr
    staff -->|"Edits catalog, content, schedules"| kedr
    kedr -->|"Imports catalog and customers, sends orders (SOAP)"| onec
    kedr -->|"Uploads and links pictures (FTP / HTTPS)"| cdn
    kedr -->|"Sends orders and alerts (Bot API)"| telegram
    kedr -->|"Runs on, is ticked by"| gcp
```

## Level 2: Containers

```mermaid
flowchart TB
    buyer["<b>Buyer</b><br/>[Person]"]
    manager["<b>Manager</b><br/>[Person]"]
    staff["<b>Staff</b><br/>[Person]"]

    subgraph cf["Cloudflare"]
        site["<b>Public site</b><br/>[Next.js on Workers, kedr-site]<br/>classkedr.com.ua, server-rendered, SEO"]
        proxy["<b>API proxy</b><br/>[Cloudflare Worker]<br/>api.classkedr.com.ua, adds proxy secret and client IP"]
        admin["<b>Admin</b><br/>[React SPA on Cloudflare Pages]<br/>admin.classkedr.com.ua, noindex, cookie session"]
    end

    mobile["<b>Кедр Менеджер</b><br/>[Expo / React Native app]<br/>Offline-first, bearer tokens"]

    subgraph gcp["Google Cloud (europe-central2)"]
        api["<b>Host.Api</b><br/>[ASP.NET Core, Cloud Run service]<br/>Minimal API of all modules"]
        jobs["<b>Host.Jobs</b><br/>[.NET console, Cloud Run Jobs]<br/>--job=migrate, --job=tasks"]
        scheduler["<b>kedr-tick</b><br/>[Cloud Scheduler]<br/>Every 5 minutes, OIDC"]
        db[("<b>PostgreSQL</b><br/>[Database]<br/>One schema per module")]
        secrets["<b>Secret Manager</b>"]
        observability["<b>Cloud Logging + Cloud Trace</b><br/>JSON logs with trace id, OTLP traces"]
    end

    onec["<b>1C</b><br/>[External, SOAP]"]
    cdn["<b>CDN</b><br/>[External, FTP + HTTPS]"]
    telegram["<b>Telegram</b><br/>[External]"]

    buyer --> site
    staff --> admin
    manager --> mobile
    manager --> admin
    site -->|"Reads catalog and content (HTTPS, OpenAPI)"| proxy
    admin -->|"JSON + cookie + CSRF"| proxy
    mobile -->|"JSON + bearer token"| proxy
    proxy --> api
    scheduler -->|"POST /internal/scheduler/tick"| api
    api -->|"Starts kedr-tasks execution"| jobs
    api --> db
    jobs --> db
    api --> cdn
    api -->|"Outbox: quick orders, orders that never reached 1C"| telegram
    api -->|"Outbox: manager orders"| onec
    jobs -->|"Catalog and customer import"| onec
    api -->|"Asks the site to refresh"| site
    api -.-> secrets
    api -.->|"Console JSON logs; traces (OTLP/HTTP, ADR-0014)"| observability
    jobs -.-> secrets
```

The old Angular storefront (Cloudflare Pages project `kedr-web`) still serves pages until the domain cutover; it is not drawn.

## Level 3: Components of the backend

```mermaid
flowchart LR
    subgraph host["Host.Api / Host.Jobs (composition only)"]
        compose["Add&lt;Module&gt;Module() calls,<br/>middleware, auth, OpenAPI"]
    end

    subgraph modules["Business modules (each: Api, Application, Contracts, Domain, Infrastructure)"]
        catalog["<b>Catalog</b><br/>products, categories, prices,<br/>attributes, 1C import"]
        sales["<b>Sales</b><br/>customers, orders, quick order,<br/>manager catalog"]
        identity["<b>Identity</b><br/>accounts, roles, policies,<br/>session and tokens"]
        content["<b>Content</b><br/>articles, pages, SEO,<br/>gallery, site refresh"]
        media["<b>Media</b><br/>picture library on the CDN"]
        work["<b>Work</b><br/>staff tasks and change log"]
        scheduling["<b>Scheduling</b><br/>background tasks, tick"]
    end

    bb["<b>BuildingBlocks</b><br/>Result mapping, Mediator behaviors,<br/>outbox, shared primitives"]

    compose --> modules
    sales -->|"Catalog.Contracts"| catalog
    sales -->|"Identity.Contracts"| identity
    catalog -->|"Media.Contracts"| media
    content -->|"Media.Contracts"| media
    catalog -.->|"Identity.Contracts (policies)"| identity
    content -.->|"Identity.Contracts (policies)"| identity
    media -.->|"Identity.Contracts (policies)"| identity
    work -.->|"Identity.Contracts (policies)"| identity
    scheduling -.->|"Identity.Contracts (policies)"| identity
    compose -->|"Scheduling.Contracts (run journal)"| scheduling
    modules --> bb
```

Solid arrows: a module reads or calls another through its `Contracts`. Dotted: only policy and role names. No module references another module's Application, Domain or Infrastructure; architecture tests enforce it ([ADR-0004](../decisions/0004-contracts-and-business-folders.md)).

Inside a module the code follows the [use-case flow](../runtime/use-case-flow.md): endpoint → command or query → Mediator pipeline → handler → aggregate → `DbContext` of the module's schema.
