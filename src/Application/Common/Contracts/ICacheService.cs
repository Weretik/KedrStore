namespace Application.Common.Interfaces
{
    public interface ICacheService
    {
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        void Set<T>(string key, T value, TimeSpan? expirationTime = null);

        Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default);
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expirationTime = null);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default);
        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        bool Exists(string key);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        void RemoveByPattern(string pattern);
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
        void Clear();
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
