using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces
{
    /// <summary>
    /// Интерфейс контекста базы данных приложения
    /// </summary>
    /// <remarks>
    /// Этот интерфейс определяет контракт для доступа к базе данных из слоя приложения.
    /// Конкретная реализация будет находиться в слое инфраструктуры.
    /// </remarks>
    public interface IApplicationDbContext
    {
        /// <summary>
        /// Предоставляет доступ к инфраструктуре DbContext
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// Получает DbSet для указанного типа сущности
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности</typeparam>
        /// <returns>DbSet для указанного типа сущности</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        /// <summary>
        /// Получает информацию об отслеживании сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Информация об отслеживании сущности</returns>
        EntityEntry Entry(object entity);

        /// <summary>
        /// Получает информацию об отслеживании сущности определенного типа
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности</typeparam>
        /// <param name="entity">Сущность</param>
        /// <returns>Информация об отслеживании сущности</returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Сохраняет все изменения, внесенные в контекст
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество затронутых записей</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Отменяет все текущие изменения и сбрасывает отслеживание сущностей
        /// </summary>
        void DiscardChanges();
    }
}
