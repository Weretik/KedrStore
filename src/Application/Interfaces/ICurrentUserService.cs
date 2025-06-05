using System.Security.Claims;

namespace Application.Interfaces
{
    /// <summary>
    /// Сервис для получения информации о текущем пользователе
    /// </summary>
    /// <remarks>
    /// Этот интерфейс предоставляет абстракцию для доступа к информации о текущем пользователе
    /// из различных частей приложения, не завися от конкретной реализации аутентификации.
    /// </remarks>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Получает идентификатор текущего пользователя
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Проверяет, аутентифицирован ли текущий пользователь
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Имя текущего пользователя
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Email текущего пользователя
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Роли текущего пользователя
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// Все утверждения (claims) текущего пользователя
        /// </summary>
        IEnumerable<Claim> Claims { get; }

        /// <summary>
        /// Проверяет, находится ли пользователь в указанной роли
        /// </summary>
        /// <param name="role">Название роли</param>
        /// <returns>true, если пользователь находится в указанной роли, иначе false</returns>
        bool IsInRole(string role);

        /// <summary>
        /// Проверяет, имеет ли пользователь указанное разрешение
        /// </summary>
        /// <param name="permission">Название разрешения</param>
        /// <returns>true, если пользователь имеет указанное разрешение, иначе false</returns>
        bool HasPermission(string permission);

        /// <summary>
        /// Получает значение указанного claim
        /// </summary>
        /// <param name="claimType">Тип claim</param>
        /// <returns>Значение claim или null, если claim не найден</returns>
        string GetClaimValue(string claimType);
    }
}
