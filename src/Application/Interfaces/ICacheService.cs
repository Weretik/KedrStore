namespace Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с кэшем
    /// </summary>
    /// <remarks>
    /// Этот интерфейс предоставляет абстракцию для кэширования данных,
    /// позволяя использовать различные реализации кэша (например, Redis, in-memory).
    /// </remarks>
    public interface ICacheService
    {
        /// <summary>
        /// Получает объект из кэша
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <returns>Объект из кэша или значение по умолчанию, если объект не найден</returns>
        T Get<T>(string key);

        /// <summary>
        /// Асинхронно получает объект из кэша
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Объект из кэша или значение по умолчанию, если объект не найден</returns>
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Устанавливает объект в кэш
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="value">Объект для кэширования</param>
        /// <param name="expirationTime">Время жизни объекта в кэше</param>
        void Set<T>(string key, T value, TimeSpan? expirationTime = null);

        /// <summary>
        /// Асинхронно устанавливает объект в кэш
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="value">Объект для кэширования</param>
        /// <param name="expirationTime">Время жизни объекта в кэше</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает объект из кэша, или создает его если он отсутствует
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="factory">Функция для создания объекта, если он отсутствует в кэше</param>
        /// <param name="expirationTime">Время жизни объекта в кэше</param>
        /// <returns>Объект из кэша или новый созданный объект</returns>
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expirationTime = null);

        /// <summary>
        /// Асинхронно получает объект из кэша, или создает его если он отсутствует
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="factory">Функция для создания объекта, если он отсутствует в кэше</param>
        /// <param name="expirationTime">Время жизни объекта в кэше</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Объект из кэша или новый созданный объект</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет объект из кэша
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        void Remove(string key);

        /// <summary>
        /// Асинхронно удаляет объект из кэша
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет наличие объекта в кэше
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <returns>true, если объект найден в кэше, иначе false</returns>
        bool Exists(string key);

        /// <summary>
        /// Асинхронно проверяет наличие объекта в кэше
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true, если объект найден в кэше, иначе false</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет все объекты из кэша, соответствующие шаблону ключа
        /// </summary>
        /// <param name="pattern">Шаблон ключа</param>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Асинхронно удаляет все объекты из кэша, соответствующие шаблону ключа
        /// </summary>
        /// <param name="pattern">Шаблон ключа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Очищает весь кэш
        /// </summary>
        void Clear();

        /// <summary>
        /// Асинхронно очищает весь кэш
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
