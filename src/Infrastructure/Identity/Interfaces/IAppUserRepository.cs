using Domain.Identity.ValueObjects;
using Infrastructure.Identity.Entities;

namespace Infrastructure.Identity.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для работы с пользователями приложения
    /// </summary>
    public interface IAppUserRepository
    {
        /// <summary>
        /// Получить пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Пользователь, если найден; иначе null</returns>
        Task<AppUser?> GetByIdAsync(AppUserId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить пользователя по имени пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Пользователь, если найден; иначе null</returns>
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить пользователя по адресу электронной почты
        /// </summary>
        /// <param name="email">Адрес электронной почты</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Пользователь, если найден; иначе null</returns>
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохранить нового пользователя
        /// </summary>
        /// <param name="user">Пользователь для сохранения</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Идентификатор созданного пользователя</returns>
        Task AddAsync(AppUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить существующего пользователя
        /// </summary>
        /// <param name="user">Пользователь с обновленными данными</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя для удаления</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        Task DeleteAsync(AppUserId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование пользователя с указанным идентификатором
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>true если пользователь существует; иначе false</returns>
        Task<bool> ExistsAsync(AppUserId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование пользователя с указанным именем
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>true если пользователь существует; иначе false</returns>
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование пользователя с указанным email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>true если пользователь существует; иначе false</returns>
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
