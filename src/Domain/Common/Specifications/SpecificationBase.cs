namespace Domain.Common.Specifications;

public abstract class SpecificationBase<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        return ToExpression().Compile().Invoke(entity);
    }

    public SpecificationBase<T> And(SpecificationBase<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    public SpecificationBase<T> Or(SpecificationBase<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    public SpecificationBase<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

