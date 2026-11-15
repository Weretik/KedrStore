# ADR-0011: Pictures live on the CDN; the Media module keeps a library of them by id

- Status: accepted
- Date: 2026-11-15 (recorded from the current code; [media spec 001](../../specs/media/001-media-library/README.md))
- Owner: Виталий (project owner)

## Context

Product photos, schemes, article covers and avatars must load fast on the site and in the app. The CDN (`images-kedr.cdn.express`) is already paid for and accepts uploads over FTP. Photos were referenced by raw URL in many tables, so moving a file broke links, and nobody knew which files were in use.

## Alternatives considered

- Store pictures in PostgreSQL or Cloud Storage: rejected; the CDN is already in place and cheaper for traffic.
- Keep raw URLs in each module: rejected; no inventory, broken links after a move.

## Decision

- The Media module owns a library of file records (CDN key, name, type, size, dimensions) and virtual folders that live only in the database, in schema `media`.
- Files are stored unchanged at `library/<yyyy>/<MM>/<guid>.<ext>` over FTP; moving or renaming a folder never changes a CDN URL.
- Other modules store only a `MediaFileId` and read URLs through `Media.Contracts` (`IMediaFileLookup`, `IMediaFileUploader`, `IMediaFolderArranger`).
- A job (`--job=media-cdn-sync`) registers files that already sit on the CDN.

## Why

- One inventory of pictures, with stable URLs.
- Modules do not know the CDN or FTP.

## Trade-offs

- FTP is a slow, old protocol; uploads go through the API.
- Without CDN credentials uploads answer 503.

## Sources and checks

- [Media library](../../features/media/media-library.md), [CDN sync runbook](../../operations/jobs/media-cdn-sync-runbook.md).
