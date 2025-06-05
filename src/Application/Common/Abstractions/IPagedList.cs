using System.Collections.Generic;

namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для представления пагинированного списка результатов
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
        /// Есть ли предыдущая страница
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Есть ли следующая страница
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Элементы текущей страницы
        /// </summary>
        IReadOnlyList<T> Items { get; }
    }
}
