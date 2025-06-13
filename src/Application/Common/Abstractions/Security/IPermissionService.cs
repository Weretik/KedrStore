namespace Application.Common.Abstractions.Security;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);
}
