namespace Application.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(pageNumber, pageSize);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, totalCount, pageNumber, pageSize);
    }

    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        IMapper mapper,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
        where TResult : class
    {
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<TResult>.Empty(pageNumber, pageSize);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PagedResult<TResult>.Create(items, totalCount, pageNumber, pageSize);
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        SortDirection sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = sortDirection == SortDirection.Asc ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        var genericMethod = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda })!;
    }
}
