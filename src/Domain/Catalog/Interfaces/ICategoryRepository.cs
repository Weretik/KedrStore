namespace Domain.Catalog.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task<List<Category>> GetChildrenAsync(CategoryId parentId, CancellationToken cancellationToken = default);
    Task<List<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(CategoryId id, CancellationToken cancellationToken = default);
}
