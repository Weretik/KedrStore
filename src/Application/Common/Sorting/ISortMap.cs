namespace Application.Common.Sorting;

public interface ISortMap<TEntity>
{
    IReadOnlyDictionary<string, Expression<Func<TEntity, object?>>> Keys { get; }
    string DefaultKey { get; }
}
