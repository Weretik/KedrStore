using System.Collections.Generic;

namespace Application.Common.Abstractions.Common
{
    /// <summary>
    /// Интерфейс для постраничного списка элементов
    /// </summary>
    /// <typeparam name="T">Тип элементов в списке</typeparam>
    public interface IPagedList<out T>
    {
        /// <summary>
        /// Текущий номер страницы
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Размер страницы
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Общее количество элементов
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Общее количество страниц
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Признак наличия предыдущей страницы
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Признак наличия следующей страницы
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Элементы текущей страницы
        /// </summary>
        IReadOnlyList<T> Items { get; }
    }
}
