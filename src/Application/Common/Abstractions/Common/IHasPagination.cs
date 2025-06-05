namespace Application.Common.Abstractions.Common
{
    /// <summary>
    /// Интерфейс для объектов, поддерживающих пагинацию
    /// </summary>
    public interface IHasPagination
    {
        /// <summary>
        /// Номер страницы (начиная с 1)
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Размер страницы
        /// </summary>
        int PageSize { get; }
    }
}
