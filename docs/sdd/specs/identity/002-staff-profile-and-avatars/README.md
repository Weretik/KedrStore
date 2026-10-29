# 002 — Staff profile and avatars

- **Module:** Identity (profile use cases, avatar storage); Host.Api (configuration, deploy secrets)
- **Status:** Backend implemented 2026-10-29; kedr-web counterpart in progress
- **Created:** 2026-10-29
- **Source:** user request in the project thread of 2026-10-29 (profile page, avatar upload, Users page avatars, header menu)
- **Depends on:** [001 authentication rollout](../001-authentication-rollout/README.md) (cookie session, staff accounts, `CanManageUsers`)
- **Frontend counterpart:** kedr-web Admin "Кабінет" page, header profile menu, avatars on the Users page

## Goal and scope

Every staff account (Admin or Manager) has a profile in Admin: it sees its email, name, role and avatar, and changes what its role allows. Avatars are stored on the company CDN (CDN Express, uploaded over FTP) and shown in the header and on the Users page. Without an avatar, the client draws the first letter of the name on a colour derived from the name.

**Out of scope:** profiles for customers (role `User`), storefront and Mobile screens, deleting staff accounts, password reset by email, email confirmation. Customers do not use the system yet.

## Product decisions (2026-10-29)

- **D-1 — storage.** Avatars go to the existing CDN Express storage over FTP, with a separate FTP user whose access directory is `/avatars`, so a leaked key cannot touch product photos.
- **D-2 — image limits.** JPEG, PNG or WebP, at most 2 MB, at least 128×128 px. The server crops the centre square and stores a 256×256 WebP.
- **D-3 — own changes.** A Manager changes only its own name and avatar. An Admin changes its own name, avatar, email and password; email and password changes need the current password.
- **D-4 — Users page.** An Admin changes the name, email, password and avatar of **managers only**, never of another Admin. Deleting accounts is not offered.
- **D-5 — after a change.** When an Admin changes a manager's email or password, the manager is signed out everywhere. A replaced or removed avatar file is deleted from the CDN. Every change is confirmed in a dialog in the client.

## Product rules

- **R-001.** `GET /api/profile` returns the signed-in staff account: id, email, full name, role and avatar URL (or `null`). A customer (`User`) gets `403`.
- **R-002.** A staff account sets its own full name (1–100 characters after trimming).
- **R-003.** A staff account uploads its own avatar. The file must be at most 2 MB, decode as JPEG, PNG or WebP, and be at least 128×128 px and at most 24 megapixels; otherwise `400` with a message saying which limit failed. The stored image is the centre square resized to 256×256 WebP, with EXIF orientation applied and metadata removed.
- **R-004.** A staff account removes its own avatar; the profile then has no avatar URL. Removing a missing avatar succeeds.
- **R-005.** Each upload gets a new file name, so CDN caches never show an old picture. After the database points to the new file, the previous file is deleted; a failed delete is logged and does not fail the request.
- **R-006.** Only an Admin changes its own email or password, and only with the correct current password (`400` otherwise, counted as a failed attempt for lockout). A Manager gets `403`. A new email already used by another account returns `409`. The password follows the staff password rules (8–128).
- **R-007.** After an Admin changes its own email or password, the current browser session stays signed in and every other session (other browsers and mobile refresh sessions) ends.
- **R-008.** On the Users page an Admin changes a manager's name, email, avatar (upload or remove) and password. Any of these operations on an Admin account, including the acting one, returns `403`. Role change and blocking keep the rules of 001 R-011.
- **R-009.** When an Admin changes a manager's email or password, all of the manager's sessions end (security stamp changes, refresh sessions are revoked).
- **R-010.** `GET /api/auth/session/me` and `GET /api/admin/users` include `avatarUrl` (nullable). Existing fields do not change.
- **R-011.** If avatar storage is not configured, upload returns `503` and the rest of the profile works.

## Acceptance scenarios

| ID | Given / When / Then |
| --- | --- |
| **SC-001** | Given a signed-in manager; when it reads its profile; then it sees its email, name, role `Manager` and `avatarUrl: null`. A customer gets `403`; no session gets `401`. |
| **SC-002** | Given a manager; when it sets its name to " Олена "; then the profile and `me` return "Олена". An empty name returns `400`. |
| **SC-003** | Given a manager; when it uploads a 1000×600 JPEG; then the storage receives a 256×256 WebP under the manager's id and the profile returns its public URL. |
| **SC-004** | Given a manager; when it uploads a 3 MB file, a 100×100 image, or a text file; then `400` with the failed limit, and nothing is stored. |
| **SC-005** | Given a manager with an avatar; when it uploads another one; then the URL changes and the old file is deleted. When it removes the avatar; then `avatarUrl` is `null` and the file is deleted. |
| **SC-006** | Given an Admin; when it changes its email with the right current password; then it can sign in with the new email, its current browser session still works, and its other refresh sessions are revoked. A wrong password returns `400`; an email of another account returns `409`. |
| **SC-007** | Given an Admin; when it changes its own password with the right current password; then the new password works and other sessions end. A manager calling the same endpoints gets `403`. |
| **SC-008** | Given an Admin and a manager; when the Admin changes the manager's name, email, avatar or password on the Users page; then the change is saved; for email and password the manager's refresh sessions are revoked. |
| **SC-009** | Given two Admins; when one changes the other's name, email, avatar or password through `/api/admin/users/{id}/…`; then `403`. The same call on its own id returns `403`. |
| **SC-010** | Given staff with and without avatars; when the Users list and `me` are read; then each row has `avatarUrl` (URL or `null`). |
| **SC-011** | Given no avatar storage configuration; when a staff account uploads; then `503`; name changes still work. |

## Design

**Data.** `AspNetUsers.AvatarPath varchar(200) null` — the file key relative to the FTP access directory, for example `3f2a…/8c1d….webp`. Migration `AddUserAvatarPath` in Identity; additive, so the API before and after the migration both work (rollback: drop the column). The URL is built at read time: `{PublicBaseUrl}/{PublicPathPrefix}/{AvatarPath}`, so moving the CDN host does not need data changes.

**Configuration** (`AvatarStorage` section, `AvatarStorageOptions`): `PublicBaseUrl` (`https://images-kedr.cdn.express`), `PublicPathPrefix` (`avatars`), `Ftp:Host`, `Ftp:Port` (21), `Ftp:Username`, `Ftp:Password`, `Ftp:Encryption` (`Auto` tries explicit TLS and falls back to plain FTP). Storage counts as configured when host, username, password and public base URL are set. The password comes from secret `cdn-avatars-ftp-password` (`AvatarStorage__Ftp__Password`); host, user and URLs are plain environment variables in the deploy workflow.

**Image processing** (`AvatarImageProcessor`, Identity.Infrastructure, SkiaSharp — MIT licence, native Linux assets without dependencies for the Debian `aspnet` image): read at most 2 MB, decode with `SKCodec`, check format and size, apply `EncodedOrigin`, crop the centre square, resize to 256 with high-quality sampling, encode WebP quality 85. The result never contains source metadata.

**Storage port.** `IAvatarStorage` (Infrastructure, used only by `AvatarService`) with `SaveAsync(key, bytes)` and `DeleteAsync(key)`; `FtpAvatarStorage` (Infrastructure, FluentFTP, MIT) creates the user directory if needed and uploads; `UnconfiguredAvatarStorage` is registered when options are incomplete and makes upload return `503`. Integration tests replace the port with an in-memory fake.

**Use cases** (`Identity.Application/Features/Profile/*`, one folder each): `GetProfile`, `ChangeOwnName`, `UploadOwnAvatar`, `RemoveOwnAvatar`, `ChangeOwnEmail`, `ChangeOwnPassword`. Users page additions in `Features/Users/*`: `ChangeStaffName`, `ChangeStaffEmail`, `UploadStaffAvatar`, `RemoveStaffAvatar`; existing `SetStaffPassword` gains the manager-only rule. A shared `AvatarService` (Infrastructure) does process → upload → save key → delete old, for both own and staff uploads. `StaffAccountService` already holds create/role/block/password; the new manager edits go to a separate `StaffProfileEditService` so that file does not grow past one responsibility. Own-profile operations live in `ProfileService`.

**Session effects.** Own email/password change: Identity updates the security stamp; the service then calls `SignInManager.RefreshSignInAsync` for the current cookie and revokes every refresh session of the user. Manager email/password change by an Admin: security stamp update plus `RevokeAllActiveByUserAsync` (as 001 does for blocking). Email change sets `Email` and `UserName` together, keeps `EmailConfirmed = true`.

**HTTP** (contract: [profile](../../../contracts/identity/profile.openapi.yaml), [staff accounts](../../../contracts/identity/users.openapi.yaml)). Unsafe cookie requests carry `X-CSRF-Token` as in 001.

| Method and route | operationId | Policy | Success | Errors |
| --- | --- | --- | --- | --- |
| `GET /api/profile` | `getMyProfile` | `RequireManagerRole` | `200 ProfileResponse` | 401, 403 |
| `PUT /api/profile/name` | `changeMyName` | `RequireManagerRole` | `204` | 400 |
| `PUT /api/profile/avatar` (multipart `file`) | `uploadMyAvatar` | `RequireManagerRole` | `200 AvatarResponse` | 400, 413, 503 |
| `DELETE /api/profile/avatar` | `removeMyAvatar` | `RequireManagerRole` | `204` | — |
| `PUT /api/profile/email` | `changeMyEmail` | `RequireAdminRole` | `204` | 400, 403, 409 |
| `PUT /api/profile/password` | `changeMyPassword` | `RequireAdminRole` | `204` | 400, 403 |
| `PUT /api/admin/users/{id}/name` | `changeStaffName` | `CanManageUsers` | `204` | 400, 403, 404 |
| `PUT /api/admin/users/{id}/email` | `changeStaffEmail` | `CanManageUsers` | `204` | 400, 403, 404, 409 |
| `PUT /api/admin/users/{id}/avatar` (multipart) | `uploadStaffAvatar` | `CanManageUsers` | `200 AvatarResponse` | 400, 403, 404, 413, 503 |
| `DELETE /api/admin/users/{id}/avatar` | `removeStaffAvatar` | `CanManageUsers` | `204` | 403, 404 |
| `PUT /api/admin/users/{id}/password` (existing) | `setStaffPassword` | `CanManageUsers` | `204` | adds 403 for an Admin target |

All writes are idempotent for the same input except avatar upload, which creates a new file each time. Avatar request bodies are limited to 2 MB + multipart overhead (`RequestSizeLimit`), so larger bodies are refused before being read.

**Compatibility.** Additive fields and routes. The only narrowing is R-008 for `setStaffPassword` on Admin targets; kedr-web hides the action for Admin rows in the same release.

## Risks

- FTP is plain text if the CDN does not offer TLS; `Auto` uses TLS when available. The dedicated FTP user limited to `/avatars` (D-1) bounds the damage.
- The secret `cdn-avatars-ftp-password` must exist before the deploy that references it, otherwise Cloud Run refuses the revision.
- SkiaSharp adds a native library to the API image (~10 MB).

## Planned file map

| Path | State | Responsibility |
| --- | --- | --- |
| `src/Identity/Identity.Infrastructure/Entities/AppUser.cs`, `Configuration/AppUserConfiguration.cs` | Done | `AvatarPath` column |
| `src/Identity/Identity.Infrastructure/Migrations/*_AddUserAvatarPath.cs` | Done | Additive column |
| `src/Identity/Identity.Application/Features/Profile/{GetProfile,ChangeOwnName,UploadOwnAvatar,RemoveOwnAvatar,ChangeOwnEmail,ChangeOwnPassword}/` | Done | Own-profile use cases and validators |
| `src/Identity/Identity.Application/Features/Users/{ChangeStaffName,ChangeStaffEmail,UploadStaffAvatar,RemoveStaffAvatar}/` | Done | Manager edits by an Admin |
| `src/Identity/Identity.Application/Features/Avatars/` | Done | `AvatarUpload` input, `AvatarResult`, `AvatarRules` (limits) |
| `src/Identity/Identity.Application/Services/IProfileService.cs`, `IStaffProfileEditService.cs` | Done | Ports for the use cases |
| `src/Identity/Identity.Infrastructure/Services/Profile/ProfileService.cs` | Done | Own profile with `UserManager`, `SignInManager` |
| `src/Identity/Identity.Infrastructure/Services/Users/{StaffProfileEditService,ManagedStaffResolver,AccountEmailWriter}.cs` | Done | Manager-only edits, Admin-target refusal, email change shared with the own profile |
| `src/Identity/Identity.Infrastructure/Services/Users/StaffAccountService.cs` | Done | `avatarUrl` in list; manager-only password rule |
| `src/Identity/Identity.Infrastructure/Avatars/{AvatarImageProcessor,AvatarOrientation,AvatarService,IAvatarStorage,FtpAvatarStorage,UnconfiguredAvatarStorage,AvatarUrlBuilder}.cs`, `Options/AvatarStorageOptions.cs` | Done | Image checks and conversion, EXIF orientation, upload pipeline, storage port and FTP, URL |
| `src/Identity/Identity.Api/Identity.Api/Controllers/ProfileController.cs` | Done | `/api/profile` |
| `src/Identity/Identity.Api/Identity.Api/Controllers/AdminUsersController.cs` | Done | New staff edit routes |
| `src/Identity/Identity.Api/Identity.Api/Contracts/{Profile,Users,Auth}/*` | Done | Requests and responses, `avatarUrl` |
| `docs/sdd/contracts/identity/profile.openapi.yaml`, `users.openapi.yaml`, `session.openapi.yaml`, `openapi.yaml` | Done | Contract |
| `.github/workflows/deploy-cloudrun.yml`, `src/Bootstrapper/Host.Api/appsettings.json` | Done | Secret `AvatarStorage__Ftp__Password`; host, user and URLs in `appsettings.json` |
| `tests/IntegrationTests/Identity/Api/ProfileApiTests.cs`, `AdminUsersProfileApiTests.cs` | Done | SC-001…SC-011 through the API with an in-memory storage |
| `tests/IntegrationTests/Identity/Avatars/AvatarImageProcessorTests.cs` | Done | Limits, crop, orientation, WebP output; in IntegrationTests because UnitTests do not reference Identity.Infrastructure |
| `docs/sdd/features/identity/staff-profile.md`, `features/README.md`, `staff-accounts.md`, `architecture/data-model/identity-access.md` | Done | Current behavior |

## Tasks and traceability

| Task | Scenarios | Depends on | Test level |
| --- | --- | --- | --- |
| **EN-001** User creates the `avatars` folder, FTP user and secret `cdn-avatars-ftp-password` | SC-003 in production | — | Manual after deploy |
| **TS-001** `AvatarPath` column and migration | SC-001, SC-010 | — | Migration script review; integration |
| **TS-002** Image processor and limits | SC-003, SC-004 | — | Unit |
| **TS-003** Storage port, FTP adapter, unconfigured fallback, URL builder | SC-003, SC-005, SC-011 | TS-002 | Unit (URL); integration with fake |
| **TS-004** Own profile API (read, name, avatar) | SC-001…SC-005, SC-011 | TS-001, TS-003 | Integration (API) |
| **TS-005** Own email and password for Admin | SC-006, SC-007 | TS-004 | Integration (API) |
| **TS-006** Manager edits on the Users API and manager-only password | SC-008, SC-009 | TS-003 | Integration (API) |
| **TS-007** `avatarUrl` in `me` and staff list; contracts; deploy config | SC-010 | TS-001 | Integration + contract |
| **TS-008** Feature map and data-model docs | — | TS-004…TS-007 | Link check |

## Verification evidence

Backend, 2026-10-29 (branch `claude/project-thread-0i1fvf`):

- EN-001 done by the user: folder `/avatars`, FTP user `cdn3245_avatars` limited to it, secret `cdn-avatars-ftp-password` in Google Cloud and local user secrets.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Identity|FullyQualifiedName~ApiContractTests"` against local PostgreSQL 16 (`KEDR_IDENTITY_TEST_POSTGRES_CONNECTION`): 54 passed. Covers SC-001…SC-011, the existing session and staff-account tests, and the contract tests.
- `dotnet test tests/UnitTests/UnitTests.csproj`: 106 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 10 passed.
- Migration `AddUserAvatarPath` adds one nullable `character varying(200)` column; the snapshot diff has no other change.
- Changed existing test: `AdminUsersApiTests.Admin_Changes_Role_And_Sets_Password` now sets the password before promoting the manager, because R-008 forbids setting an administrator's password.
- Not verified here: the real FTP upload to CDN Express (no credentials in the test environment). Check after deploy by uploading an avatar in Admin.
