using Catalog.Application.Contracts.Media;

namespace IntegrationTests.Catalog.ProductMedia;

/// <summary>An in-memory media library with the same directory rule as the real one.</summary>
internal sealed class FakeProductMediaLibrary : IProductMediaLibrary
{
    public Dictionary<long, ProductMediaLibraryFile> Files { get; } = [];

    public ProductMediaLibraryFile Add(long id, string key)
        => Files[id] = new ProductMediaLibraryFile(id, key, key[(key.LastIndexOf('/') + 1)..],
            $"https://images-kedr.cdn.express/{key}");

    public Task<IReadOnlyList<ProductMediaLibraryFile>> GetByIdsAsync(IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<ProductMediaLibraryFile>>(
            ids.Distinct().Where(Files.ContainsKey).Select(id => Files[id]).ToList());

    public Task<IReadOnlyList<ProductMediaLibraryFile>> ListInDirectoryAsync(string directory,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<ProductMediaLibraryFile>>(Files.Values
            .Where(file => file.StorageKey.StartsWith(directory + "/", StringComparison.Ordinal) &&
                           file.StorageKey.IndexOf('/', directory.Length + 1) < 0)
            .ToList());
}
