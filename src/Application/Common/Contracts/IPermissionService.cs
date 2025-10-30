namespace Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);
}
