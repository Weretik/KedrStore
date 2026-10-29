using Ardalis.Result;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Users;

/// <summary>Changes the sign-in email; the user name follows it, as for every staff account.</summary>
public sealed class AccountEmailWriter(UserManager<AppUser> userManager)
{
    public async Task<Result> ChangeAsync(AppUser user, string email)
    {
        var normalized = email.Trim();
        if (string.Equals(user.Email, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success();
        }

        var owner = await userManager.FindByEmailAsync(normalized);
        if (owner is not null && owner.Id != user.Id)
        {
            return Result.Conflict("A user with this email already exists.");
        }

        user.Email = normalized;
        user.UserName = normalized;
        user.EmailConfirmed = true;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            return Result.Invalid(updated.Errors.Select(error => new ValidationError(error.Description)).ToList());
        }

        // Sessions that carry the old email end at the next security stamp check.
        await userManager.UpdateSecurityStampAsync(user);
        return Result.Success();
    }
}
