# Media: library folders and files

**Checked:** 2026-11-04 against `MediaDbContext` and its configurations. Schema `media`; migration history in `media."__EFMigrationsHistory"`.

```mermaid
erDiagram
    MediaFolders ||--o{ MediaFolders : "ParentId (restrict)"
    MediaFolders ||--o{ MediaFiles : "FolderId (restrict)"
    MediaFolders {
        bigint Id PK "identity"
        varchar100 Name "unique among siblings, checked by the handler"
        bigint ParentId FK "null = library root"
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
    MediaFiles {
        bigint Id PK "identity; other modules link by this ID"
        bigint FolderId FK "null = library root"
        varchar200 FileName "display name"
        varchar500 StorageKey UK "CDN path from its root"
        varchar100 ContentType
        bigint SizeBytes
        int Width "null for files registered by the CDN sync"
        int Height
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
```

Indexes: `MediaFolders(ParentId)`, `MediaFiles(StorageKey)` unique, `MediaFiles(FolderId, FileName)` for folder pages ordered by name. Folders are virtual: the CDN path in `StorageKey` never changes when a file or folder moves. The public URL is built at read time from `MediaStorage:PublicBaseUrl`.
