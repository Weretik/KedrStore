using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Application.Common.Mapping
{
    /// <summary>
    /// Расширения для работы с маппингом
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Проецирует IQueryable в IQueryable нового типа
        /// </summary>
        public static IQueryable<TDestination> ProjectToType<TDestination>(
            this IQueryable source,
            IConfigurationProvider configuration)
        {
            return source.ProjectTo<TDestination>(configuration);
        }

        /// <summary>
        /// Преобразует коллекцию из одного типа в другой
        /// </summary>
        public static List<TDestination> MapList<TSource, TDestination>(
            this IMapper mapper,
            IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);

            return source.Select(item => mapper.Map<TDestination>(item)).ToList();
        }

        /// <summary>
        /// Асинхронно получает список с маппингом в заданный тип
        /// </summary>
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

            /// <summary>
            /// Асинхронно получает первый элемент списка с маппингом в заданный тип
            /// </summary>
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

            /// <summary>
            /// Асинхронно получает единственный элемент списка с маппингом в заданный тип
            /// </summary>
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
