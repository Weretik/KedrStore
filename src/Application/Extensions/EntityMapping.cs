namespace Application.Extensions
{
    public static class EntityMapping
    {
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
        public static TDestination ProjectTo<TDestination>(
            this IMapper mapper,
            object source)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);

            return mapper.Map<TDestination>(source);
        }

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
