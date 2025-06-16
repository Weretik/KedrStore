namespace Application.Common.Abstractions.UseCase;

public interface IQuery<TResult>
    : IRequest<TResult>, IUseCase { }
