using AutoMapper;

namespace Application.Extensions
{
    /// <summary>
    /// Расширения для маппинга сущностей
    /// </summary>
    public static class EntityMapping
    {
        /// <summary>
        /// Обновляет свойства сущности на основе данных из другой сущности
        /// </summary>
        /// <typeparam name="TSource">Тип источника</typeparam>
        /// <typeparam name="TDestination">Тип назначения</typeparam>
        /// <param name="mapper">Маппер</param>
        /// <param name="source">Исходный объект</param>
        /// <param name="destination">Объект назначения</param>
        /// <returns>Обновленный объект назначения</returns>
        public static TDestination MapTo<TSource, TDestination>(
            this IMapper mapper,
            TSource source,
            TDestination destination)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destination);

            return mapper.Map(source, destination);
        }

        /// <summary>
        /// Проецирует сущность в новый тип
        /// </summary>
        /// <typeparam name="TDestination">Тип результата</typeparam>
        /// <param name="mapper">Маппер</param>
        /// <param name="source">Исходный объект</param>
        /// <returns>Новый объект требуемого типа</returns>
        public static TDestination ProjectTo<TDestination>(
            this IMapper mapper,
            object source)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);

            return mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Обновляет список элементов, заменяя, добавляя и удаляя их по необходимости
        /// </summary>
        /// <typeparam name="TSource">Тип источника</typeparam>
        /// <typeparam name="TDestination">Тип назначения</typeparam>
        /// <typeparam name="TKey">Тип ключа для сопоставления элементов</typeparam>
        /// <param name="mapper">Маппер</param>
        /// <param name="sourceList">Исходный список</param>
        /// <param name="destinationList">Список назначения</param>
        /// <param name="sourceKeySelector">Функция выбора ключа из исходного элемента</param>
        /// <param name="destinationKeySelector">Функция выбора ключа из элемента назначения</param>
        /// <returns>Обновленный список элементов</returns>
        public static List<TDestination> SyncList<TSource, TDestination, TKey>(
            this IMapper mapper,
            List<TSource> sourceList,
            List<TDestination> destinationList,
            Func<TSource, TKey> sourceKeySelector,
            Func<TDestination, TKey> destinationKeySelector)
            where TSource : class
            where TDestination : class
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sourceList);
            ArgumentNullException.ThrowIfNull(destinationList);
            ArgumentNullException.ThrowIfNull(sourceKeySelector);
            ArgumentNullException.ThrowIfNull(destinationKeySelector);

            // Найти элементы для удаления (есть в назначении, но нет в источнике)
            var itemsToRemove = destinationList
                .Where(destItem => !sourceList.Any(srcItem =>
                    EqualityComparer<TKey>.Default.Equals(sourceKeySelector(srcItem), destinationKeySelector(destItem))))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                destinationList.Remove(item);
            }

            // Обновить существующие и добавить новые элементы
            foreach (var sourceItem in sourceList)
            {
                var sourceKey = sourceKeySelector(sourceItem);
                var existingItem = destinationList
                    .FirstOrDefault(destItem =>
                        EqualityComparer<TKey>.Default.Equals(destinationKeySelector(destItem), sourceKey));

                if (existingItem != null)
                {
                    // Обновить существующий элемент
                    mapper.Map(sourceItem, existingItem);
                }
                else
                {
                    // Добавить новый элемент
                    var newItem = mapper.Map<TDestination>(sourceItem);
                    destinationList.Add(newItem);
                }
            }

            return destinationList;
        }
    }
}
