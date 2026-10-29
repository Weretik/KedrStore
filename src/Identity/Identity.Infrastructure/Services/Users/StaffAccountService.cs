using Ardalis.Result;
using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Services.Sessions;

namespace Identity.Infrastructure.Services.Users;

public sealed class StaffAccountService(
    AppIdentityDbContext dbContext,
    UserManager<AppUser> userManager,
    RefreshSessionStore refreshSessionStore,
    ManagedStaffResolver managedStaffResolver,
    AvatarUrlBuilder avatarUrlBuilder,
    IHttpContextAccessor httpContextAccessor,
    ILogger<StaffAccountService> logger) : IStaffAccountService
{
    public async Task<IReadOnlyList<StaffAccountDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var rows = await (
                from user in dbContext.Users
                join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
                join role in dbContext.Roles on userRole.RoleId equals role.Id
                where StaffRoles.All.Contains(role.Name!)
                orderby user.Email
                select new { user.Id, user.Email, user.FullName, Role = role.Name!, user.LockoutEnd, user.AvatarPath })
            .ToListAsync(cancellationToken);

        // A user with both staff roles is listed once, with the stronger role first in StaffRoles.All.
        return rows
            .GroupBy(x => x.Id)
            .Select(group => group.OrderBy(x => IndexOfRole(x.Role)).First())
            .Select(x => new StaffAccountDto(x.Id, x.Email ?? string.Empty, x.FullName, x.Role, x.LockoutEnd > now, avatarUrlBuilder.Build(x.AvatarPath)))
            .ToList();
    }

    public async Task<Result<StaffAccountDto>> CreateAsync(
        string email,
        string fullName,
        string role,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Conflict("A user with this email already exists.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return Invalid(created);
        }

        var assigned = await userManager.AddToRoleAsync(user, role);
        if (!assigned.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Invalid(assigned);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} created with role {Role} by {ActorId}", user.Id, role, CurrentUserId());
        return Result.Success(new StaffAccountDto(user.Id, email, user.FullName, role, IsBlocked: false, AvatarUrl: null));
    }

    public async Task<Result> ChangeRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        if (userId == CurrentUserId())
        {
            return SelfChangeRejected("You cannot change your own role.");
        }

        var user = await FindStaffAsync(userId);
        if (user is null)
        {
            return Result.NotFound();
        }

        var currentRoles = (await userManager.GetRolesAsync(user)).Where(StaffRoles.IsStaffRole).ToArray();
        var removed = await userManager.RemoveFromRolesAsync(user, currentRoles);
        var added = removed.Succeeded ? await userManager.AddToRoleAsync(user, role) : removed;
        if (!added.Succeeded)
        {
            return Invalid(added);
        }

        // Ends cookie sessions at the next stamp check, so the new role applies everywhere.
        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("AUDIT: Staff account {UserId} role set to {Role} by {ActorId}", userId, role, CurrentUserId());
        return Result.Success();
    }

    public async Task<Result> SetBlockedAsync(Guid userId, bool blocked, CancellationToken cancellationToken = default)
    {
        if (blocked && userId == CurrentUserId())
        {
            return SelfChangeRejected("You cannot block your own account.");
        }

        var user = await FindStaffAsync(userId);
        if (user is null)
        {
            return Result.NotFound();
        }

        if (blocked)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await userManager.UpdateSecurityStampAsync(user);
            await refreshSessionStore.RevokeAllActiveByUserAsync(user.Id, DateTimeOffset.UtcNow, "Blocked", cancellationToken);
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} blocked={Blocked} by {ActorId}", userId, blocked, CurrentUserId());
        return Result.Success();
    }

    public async Task<Result> SetPasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        // Only managers' passwords are set here; an administrator changes their own in the profile.
        var target = await managedStaffResolver.ResolveAsync(userId);
        if (target.User is not { } user)
        {
            return target.Failure;
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, resetToken, password);
        if (!reset.Succeeded)
        {
            return Invalid(reset);
        }

        // The reset changed the security stamp; refresh sessions end now too.
        await refreshSessionStore.RevokeAllActiveByUserAsync(user.Id, DateTimeOffset.UtcNow, "PasswordReset", cancellationToken);
        logger.LogInformation("AUDIT: Staff account {UserId} password set by {ActorId}", userId, CurrentUserId());
        return Result.Success();
    }

    private async Task<AppUser?> FindStaffAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return roles.Any(StaffRoles.IsStaffRole) ? user : null;
    }

    private Guid? CurrentUserId() =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static int IndexOfRole(string role)
    {
        for (var i = 0; i < StaffRoles.All.Count; i++)
        {
            if (StaffRoles.All[i] == role)
            {
                return i;
            }
        }

        return int.MaxValue;
    }

    private static Result SelfChangeRejected(string message) =>
        Result.Invalid(new ValidationError(message));

    private static Result Invalid(IdentityResult result) =>
        Result.Invalid(result.Errors.Select(error => new ValidationError(error.Description)).ToList());
}
