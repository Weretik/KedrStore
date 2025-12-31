using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Contracts
{
    public interface IAppUserRepository
    {
        Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(AppUser user, CancellationToken cancellationToken = default);
        Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
