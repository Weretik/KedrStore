namespace Domain.Common.Specifications;

public sealed class NotSpecification<T>(
    ISpecification<T> specification)
    : SpecificationBase<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var innerExpr = specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));

        var body = Expression.Not(Expression.Invoke(innerExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
