# Staff profile and avatars

**Static code check:** 2026-10-29. **Module:** Identity. **Consumer:** React Admin "Кабінет" page, header profile menu, Users page.

Every staff account reads its profile and changes its name and avatar; an administrator also changes its own email and password with the current password. On the Users page an administrator edits a manager's name, email, password and avatar, never another administrator's. Avatars are processed to a 256×256 WebP and stored on CDN Express over FTP. Rules are in the [spec](../../specs/identity/002-staff-profile-and-avatars/README.md).

## Structure and relationships

[ProfileController](../../../../src/Identity/Identity.Api/Identity.Api/Controllers/ProfileController.cs) (`/api/profile`) dispatches the `Features/Profile` use cases to [ProfileService](../../../../src/Identity/Identity.Infrastructure/Services/Profile/ProfileService.cs). The Users page routes in [AdminUsersController](../../../../src/Identity/Identity.Api/Identity.Api/Controllers/AdminUsersController.cs) go to [StaffProfileEditService](../../../../src/Identity/Identity.Infrastructure/Services/Users/StaffProfileEditService.cs), which uses [ManagedStaffResolver](../../../../src/Identity/Identity.Infrastructure/Services/Users/ManagedStaffResolver.cs) to refuse administrator targets. Both use [AvatarService](../../../../src/Identity/Identity.Infrastructure/Avatars/AvatarService.cs): [AvatarImageProcessor](../../../../src/Identity/Identity.Infrastructure/Avatars/AvatarImageProcessor.cs) checks and converts the image (SkiaSharp), `IAvatarStorage` uploads it ([FtpAvatarStorage](../../../../src/Identity/Identity.Infrastructure/Avatars/FtpAvatarStorage.cs), or `UnconfiguredAvatarStorage` → `503` without credentials), the user's `AvatarPath` is saved and the previous file deleted. [AvatarUrlBuilder](../../../../src/Identity/Identity.Infrastructure/Avatars/AvatarUrlBuilder.cs) turns the key into the public URL for the profile, `me`, and the staff list. Email changes go through [AccountEmailWriter](../../../../src/Identity/Identity.Infrastructure/Services/Users/AccountEmailWriter.cs).

## Code structure

```text
src/Identity/
├── Identity.Api/Identity.Api/
│   ├── Controllers/ProfileController.cs  # own profile, RequireManagerRole (email/password: RequireAdminRole)
│   ├── Avatars/AvatarUploadResults.cs  # multipart limit, 200/400/503 mapping
│   └── Contracts/Profile/  # requests and responses
├── Identity.Application/
│   ├── Features/Profile/  # GetProfile, ChangeOwnName, UploadOwnAvatar, RemoveOwnAvatar, ChangeOwnEmail, ChangeOwnPassword
│   ├── Features/Users/  # ChangeStaffName, ChangeStaffEmail, UploadStaffAvatar, RemoveStaffAvatar
│   ├── Features/Avatars/  # AvatarRules (limits), AvatarUpload, AvatarResult
│   └── Services/IProfileService.cs, IStaffProfileEditService.cs
└── Identity.Infrastructure/
    ├── Avatars/  # processor, upload pipeline, FTP and unconfigured storage, URL builder
    ├── Options/AvatarStorageOptions.cs  # AvatarStorage section
    └── Services/
        ├── Profile/ProfileService.cs  # own profile, keeps the current browser session after email/password change
        └── Users/  # StaffProfileEditService, ManagedStaffResolver, AccountEmailWriter
```

## Configuration

`AvatarStorage` in `appsettings.json`: `PublicBaseUrl`, `PublicPathPrefix` (`avatars`), `Ftp:Host`, `Ftp:Port`, `Ftp:Username` (`cdn3245_avatars`, access directory `/avatars`), `Ftp:Encryption` (`Auto`). The password comes from the secret `cdn-avatars-ftp-password` (`AvatarStorage__Ftp__Password` in the [deploy workflow](../../../../.github/workflows/deploy-cloudrun.yml)) or local user secrets (`AvatarStorage:Ftp:Password`).

## Tests

[ProfileApiTests](../../../../tests/IntegrationTests/Identity/Api/ProfileApiTests.cs) and [AdminUsersProfileApiTests](../../../../tests/IntegrationTests/Identity/Api/AdminUsersProfileApiTests.cs) run through the real API against PostgreSQL with an in-memory storage. [AvatarImageProcessorTests](../../../../tests/IntegrationTests/Identity/Avatars/AvatarImageProcessorTests.cs) need no database.

`dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Identity"`

## Related sources

[Contract](../../contracts/identity/profile.openapi.yaml) · [Staff accounts](staff-accounts.md) · [Authorization](../../architecture/security/authorization/roles-and-policies.md).
