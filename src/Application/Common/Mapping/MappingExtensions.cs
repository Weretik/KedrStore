namespace Application.Common.Mapping
{
    public static class MappingExtensions
    {
        public static IQueryable<TDestination> ProjectToDto<TDestination>(
            this IQueryable source,
            IConfigurationProvider configuration)
        {
            return source.ProjectTo<TDestination>(configuration);
        }
        public static List<TDestination> MapList<TSource, TDestination>(
            this IMapper mapper,
            IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);

            return source.Select(item => mapper.Map<TDestination>(item)).ToList();
        }
            public static async Task<List<TDestination>> ProjectToListAsync<TSource, TDestination>(
                this IQueryable<TSource> queryable,
                IConfigurationProvider configuration,
                CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(queryable);
                ArgumentNullException.ThrowIfNull(configuration);

                return await queryable.ProjectTo<TDestination>(configuration)
                    .ToListAsync(cancellationToken);
            }
            public static async Task<TDestination?> ProjectToFirstOrDefaultAsync<TSource, TDestination>(
                this IQueryable<TSource> queryable,
                IConfigurationProvider configuration,
                CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(queryable);
                ArgumentNullException.ThrowIfNull(configuration);

                return await queryable.ProjectTo<TDestination>(configuration)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            public static async Task<TDestination> ProjectToSingleAsync<TSource, TDestination>(
                this IQueryable<TSource> queryable,
                IConfigurationProvider configuration,
                CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(queryable);
                ArgumentNullException.ThrowIfNull(configuration);

                return await queryable.ProjectTo<TDestination>(configuration)
                    .SingleAsync(cancellationToken);
            }
    }
}
