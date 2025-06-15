namespace Application.Common.Abstractions.Queries;

public interface IQuery<TResult>
    : IRequest<TResult>, IUseCase { }
