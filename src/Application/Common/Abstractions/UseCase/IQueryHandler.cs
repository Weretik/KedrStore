namespace Application.Common.Abstractions.UseCase
{
    public interface IQueryHandler<TQuery, TResult>
        : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>;
}
