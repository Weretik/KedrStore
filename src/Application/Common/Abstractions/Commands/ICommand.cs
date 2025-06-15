namespace Application.Common.Abstractions.Commands;

public interface ICommand<TResult> : IRequest<TResult>, IUseCase { }
public interface ICommand : ICommand<Unit> { }
