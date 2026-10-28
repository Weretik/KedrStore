using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Services;

/// <summary>
/// Administrator and manager accounts. The current user is the acting administrator.
/// </summary>
public interface IStaffAccountService
{
    Task<IReadOnlyList<StaffAccountDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<Result<StaffAccountDto>> CreateAsync(string email, string fullName, string role, string password, CancellationToken cancellationToken = default);
    Task<Result> ChangeRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<Result> SetBlockedAsync(Guid userId, bool blocked, CancellationToken cancellationToken = default);
    Task<Result> SetPasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default);
}
