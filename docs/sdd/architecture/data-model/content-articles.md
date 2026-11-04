# Content: site articles, company contacts and requisites

**Checked:** 2026-11-04 against `ContentDbContext` and its configurations. Schema `content`; migration history in `content."__EFMigrationsHistory"`.

```mermaid
erDiagram
    Articles ||--|{ ArticleTranslations : "ArticleId (cascade)"
    Articles ||--o| ArticleCovers : "ArticleId (cascade)"
    Articles {
        bigint Id PK "identity"
        varchar120 Slug UK
        date PublishedOn
        bool IsPublished
        int CoverVersion "null without a cover"
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
    ArticleTranslations {
        bigint ArticleId PK
        char2 Language PK "uk or ru"
        varchar200 Title
        varchar300 Excerpt
        varchar50000 BodyHtml "sanitized"
    }
    ArticleCovers {
        bigint ArticleId PK
        bytea Data "WebP"
        varchar64 ContentType
    }
    CompanyContacts {
        int Id PK "always 1 (check constraint)"
        varchar13 SalesPhone "E.164"
        varchar120 Email
        varchar500 TelegramUrl
        varchar13 ViberPhone "E.164"
        varchar500 MapUrl
        varchar80 CityUk
        varchar80 CityRu
        timestamptz UpdatedAt
    }
    CompanyRequisites {
        int Id PK "always 1 (check constraint)"
        varchar200 LegalName
        varchar10 Code "ЄДРПОУ or РНОКПП"
        varchar30 RegistrationExtractNumber
        varchar29 Iban "no spaces"
        varchar120 BankName
        varchar6 BankCode "МФО"
        timestamptz UpdatedAt
    }
    CompanyRequisitesChanges {
        bigint Id PK
        timestamptz ChangedAt
        varchar64 ChangedByUserId
        varchar256 ChangedByName
        jsonb Changes "field, old, new"
    }
```

Index `(IsPublished, PublishedOn)` serves the public list. The `SeedLegacyArticles` migration inserts articles 1–6 and moves the identity sequence past them. `CompanyContacts` has exactly one row, inserted by the `AddCompanyContacts` migration; `CompanyRequisites` likewise by `AddCompanyRequisites`, which also creates the change log (index on `ChangedAt`, no link to the row because there is only one). The module has no references to other modules' tables.

Features: [Site articles](../../features/content/articles.md), [Company contacts](../../features/content/company-contacts.md), [Company requisites](../../features/content/company-requisites.md).
