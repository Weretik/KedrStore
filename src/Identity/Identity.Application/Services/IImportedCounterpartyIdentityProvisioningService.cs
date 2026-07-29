namespace Identity.Application.Services;

public interface IImportedCounterpartyIdentityProvisioningService
{
    Task<Result<Guid>> EnsureUserAsync(
        ImportedCounterpartyIdentityUserData userData,
        Guid? existingIdentityUserId,
        CancellationToken cancellationToken);

    Task<Result> DeleteUserAsync(
        Guid identityUserId,
        string counterpartyId,
        CancellationToken cancellationToken);
}

public sealed record ImportedCounterpartyIdentityUserData(
    string CounterpartyId,
    string Email,
    string FullName);
