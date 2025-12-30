using Application.Common.Interfaces;

namespace BuildingBlocks.Infrastructure.Services;

public class FakePermissionService : IPermissionService
{
    public Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        // Заглушка
        return Task.FromResult(true);
    }
}
