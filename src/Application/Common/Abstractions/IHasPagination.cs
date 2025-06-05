namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для запросов, поддерживающих пагинацию
    /// </summary>
    public interface IHasPagination
    {
        /// <summary>
        /// Номер страницы (начиная с 1)
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Размер страницы (количество элементов на странице)
        /// </summary>
        int PageSize { get; }
    }
}
