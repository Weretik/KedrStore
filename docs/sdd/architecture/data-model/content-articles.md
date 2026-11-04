# Content: site articles, company contacts, requisites, documents and gallery

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
    LegalDocumentVersions ||--|{ LegalDocumentTexts : "VersionId (cascade)"
    LegalDocumentVersions {
        bigint Id PK "identity"
        varchar32 Kind "PublicOffer, PrivacyPolicy, ReturnsPolicy, DeliveryPayment"
        varchar16 Status "Draft, Published, Archived"
        int Number "null while a draft"
        date EffectiveFrom "«Редакція від»; null while a draft"
        timestamptz UpdatedAt
        varchar256 UpdatedBy
        timestamptz PublishedAt
        varchar256 PublishedBy
    }
    LegalDocumentTexts {
        bigint VersionId PK
        char2 Language PK "uk or ru"
        varchar200 Title
        varchar100000 BodyHtml "sanitized"
    }
    GalleryItems {
        bigint Id PK "identity"
        varchar16 Kind "Photo or Video"
        bigint MediaFileId "Media library file, value only; photos"
        varchar11 YoutubeId "videos"
        int Position "site order"
        varchar150 TitleUk
        varchar150 TitleRu
        varchar500 CaptionUk
        varchar500 CaptionRu
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
```

Index `(IsPublished, PublishedOn)` serves the public list. The `SeedLegacyArticles` migration inserts articles 1–6 and moves the identity sequence past them. `CompanyContacts` has exactly one row, inserted by the `AddCompanyContacts` migration; `CompanyRequisites` likewise by `AddCompanyRequisites`, which also creates the change log (index on `ChangedAt`, no link to the row because there is only one). `LegalDocumentVersions` has filtered unique indexes on `Kind` for `Status = 'Draft'` and for `Status = 'Published'` (one draft and one edition on the site per document), a unique `(Kind, Number)`, and a check that a non-draft has a number, date and publish time; `SeedLegalDocuments` inserts edition 1 of each document (from 2026-11-03) and moves the identity sequence. `GalleryItems` has a check that a photo has only `MediaFileId` and a video only `YoutubeId`, filtered unique indexes on both (a picture or video once), and an index on `Position`; `SeedGallery` inserts the seven videos the site had. `MediaFileId` is a plain value: the Media module owns the file and Content reads it through `Media.Contracts`. The module has no references to other modules' tables.

Features: [Site articles](../../features/content/articles.md), [Company contacts](../../features/content/company-contacts.md), [Company requisites](../../features/content/company-requisites.md), [Site documents](../../features/content/legal-documents.md), [Site gallery](../../features/content/gallery.md).
