namespace Application.Common.Abstractions.UseCase;

public interface ICommand<TResult> : IRequest<TResult>, IUseCase { }
public interface ICommand : ICommand<Unit> { }
