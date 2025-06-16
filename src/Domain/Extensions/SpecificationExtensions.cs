namespace Domain.Extensions;

public static class SpecificationExtensions
{
    public static IQueryable<T> ApplySpecification<T>(
        this IQueryable<T> query,
        ISpecification<T> specification)
        where T : class
    {
        return query.Where(specification.ToExpression());
    }
}
