using Ardalis.Result;
using Identity.Application.Services;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services;

public sealed class ImportedCounterpartyIdentityProvisioningService(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IOptions<ImportedCounterpartyIdentityOptions> options,
    ILogger<ImportedCounterpartyIdentityProvisioningService> logger)
    : IImportedCounterpartyIdentityProvisioningService
{
    private readonly ImportedCounterpartyIdentityOptions _options = options.Value;

    public async Task<Result<Guid>> EnsureUserAsync(
        ImportedCounterpartyIdentityUserData userData,
        Guid? existingIdentityUserId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        AppUser? appUser = null;

        if (existingIdentityUserId.HasValue && existingIdentityUserId.Value != Guid.Empty)
        {
            appUser = await userManager.FindByIdAsync(existingIdentityUserId.Value.ToString());
        }

        if (appUser is null)
        {
            appUser = await userManager.FindByEmailAsync(userData.Email);
        }

        if (appUser is null)
        {
            appUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = userData.Email,
                Email = userData.Email,
                FullName = userData.FullName,
                EmailConfirmed = _options.EmailConfirmed,
                LockoutEnabled = _options.LockoutEnabled
            };

            var createResult = await userManager.CreateAsync(appUser, userData.CounterpartyId);
            if (!createResult.Succeeded)
            {
                return Result.Error(FormatErrors(createResult.Errors));
            }

            await EnsureUserRoleAsync(appUser);
            logger.LogInformation(
                "Imported AppUser {UserId} created for counterparty {CounterpartyId}",
                appUser.Id,
                userData.CounterpartyId);

            return Result.Success(appUser.Id);
        }

        appUser.UserName = userData.Email;
        appUser.Email = userData.Email;
        appUser.FullName = userData.FullName;
        appUser.EmailConfirmed = _options.EmailConfirmed;
        appUser.LockoutEnabled = _options.LockoutEnabled;

        var updateResult = await userManager.UpdateAsync(appUser);
        if (!updateResult.Succeeded)
        {
            return Result.Error(FormatErrors(updateResult.Errors));
        }

        var passwordResult = await SetImportedPasswordAsync(appUser, userData.CounterpartyId);
        if (!passwordResult.IsSuccess)
        {
            return passwordResult;
        }

        await EnsureUserRoleAsync(appUser);
        return Result.Success(appUser.Id);
    }

    public async Task<Result> DeleteUserAsync(
        Guid identityUserId,
        string counterpartyId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (identityUserId == Guid.Empty)
            return Result.Success();

        var appUser = await userManager.FindByIdAsync(identityUserId.ToString());
        if (appUser is null)
            return Result.Success();

        var deleteResult = await userManager.DeleteAsync(appUser);
        if (!deleteResult.Succeeded)
        {
            return Result.Error(FormatErrors(deleteResult.Errors));
        }

        logger.LogInformation(
            "Imported AppUser {UserId} deleted for stale counterparty {CounterpartyId}",
            identityUserId,
            counterpartyId);

        return Result.Success();
    }

    private async Task<Result<Guid>> SetImportedPasswordAsync(AppUser appUser, string importedPassword)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(appUser);
        var resetResult = await userManager.ResetPasswordAsync(appUser, resetToken, importedPassword);
        if (!resetResult.Succeeded)
        {
            return Result.Error(FormatErrors(resetResult.Errors));
        }

        return Result.Success(appUser.Id);
    }

    private async Task EnsureUserRoleAsync(AppUser appUser)
    {
        if (!await roleManager.RoleExistsAsync(RoleNames.User))
            return;

        if (await userManager.IsInRoleAsync(appUser, RoleNames.User))
            return;

        var roleResult = await userManager.AddToRoleAsync(appUser, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning(
                "AppUser {UserId} was synced, but assigning role {Role} failed: {Errors}",
                appUser.Id,
                RoleNames.User,
                FormatErrors(roleResult.Errors));
        }
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
        => string.Join(", ", errors.Select(error => error.Description));
}
