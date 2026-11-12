# Content: site articles, company contacts, requisites, documents, gallery, home page blocks, the page «Співпраця» and page search texts

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
        int CoverVersion "legacy cover in ArticleCovers; null otherwise"
        bigint CoverMediaFileId "Media library file, value only; null without a cover"
        timestamptz CreatedAt
        timestamptz UpdatedAt
        varchar256 UpdatedBy "editor; null before Content 007"
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
        bytea Data "WebP; the six legacy covers only"
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
        varchar256 UpdatedBy "editor; null for the migration row"
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
    HomeFaqItems {
        bigint Id PK "identity"
        int Position "site order"
        varchar200 QuestionUk
        varchar200 QuestionRu
        varchar2000 AnswerUk
        varchar2000 AnswerRu
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
    HomeBrands {
        bigint Id PK "identity"
        int Position "site order"
        varchar100 Name
        bigint LogoMediaFileId "Media library file, value only"
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
    WholesalePages {
        int Id PK "always 1"
        jsonb Uk "texts, steps, questions"
        jsonb Ru "texts, steps, questions"
        timestamptz UpdatedAt
        varchar256 UpdatedBy "null for the seeded row"
    }
    PageSeo {
        varchar32 Page PK "SitePages name"
        varchar120 TitleUk "null keeps the site text"
        varchar120 TitleRu
        varchar300 DescriptionUk
        varchar300 DescriptionRu
        timestamptz UpdatedAt
        varchar256 UpdatedBy
    }
    SiteSeoSettings {
        int Id PK "always 1"
        varchar20 GoogleAnalyticsId "null adds nothing"
        varchar20 GoogleTagManagerId
        varchar100 GoogleSiteVerification
        varchar100 BingSiteVerification
        bigint ShareImageMediaFileId "Media file, no FK"
        timestamptz UpdatedAt
        varchar256 UpdatedBy "null for the seeded row"
    }
```

Index `(IsPublished, PublishedOn)` serves the public list. The `SeedLegacyArticles` migration inserts articles 1–6 and moves the identity sequence past them. `CompanyContacts` has exactly one row, inserted by the `AddCompanyContacts` migration; `CompanyRequisites` likewise by `AddCompanyRequisites`, which also creates the change log (index on `ChangedAt`, no link to the row because there is only one). `LegalDocumentVersions` has filtered unique indexes on `Kind` for `Status = 'Draft'` and for `Status = 'Published'` (one draft and one edition on the site per document), a unique `(Kind, Number)`, and a check that a non-draft has a number, date and publish time; `SeedLegalDocuments` inserts edition 1 of each document (from 2026-11-03) and moves the identity sequence. `GalleryItems` has a check that a photo has only `MediaFileId` and a video only `YoutubeId`, filtered unique indexes on both (a picture or video once), and an index on `Position`; `SeedGallery` inserts the seven videos the site had. `HomeFaqItems` and `HomeBrands` have an index on `Position`; `SeedHomeFaq` inserts the six home questions, brands start empty. `WholesalePages` has exactly one row (check `Id = 1`); `AddWholesalePage` inserts it with the texts the site kept in code, each language one jsonb document. `PageSeo` has one row per edited site page, created by its first save (`AddPageSeo` inserts nothing); a null value means the site keeps its own text. `SiteSeoSettings` has exactly one row (check `Id = 1`), inserted empty by `AddSiteSeoSettings`. `MediaFileId`, `LogoMediaFileId`, `CoverMediaFileId` and `ShareImageMediaFileId` are plain values: the Media module owns the file and Content reads it through `Media.Contracts`. The module has no references to other modules' tables.

Features: [Site articles](../../features/content/articles.md), [Company contacts](../../features/content/company-contacts.md), [Company requisites](../../features/content/company-requisites.md), [Site documents](../../features/content/legal-documents.md), [Site gallery](../../features/content/gallery.md), [Home page FAQ and brands](../../features/content/home-blocks.md), [Page «Співпраця»](../../features/content/wholesale-page.md), [Search texts of the site pages](../../features/content/page-seo.md), [Site SEO settings](../../features/content/site-seo-settings.md).
