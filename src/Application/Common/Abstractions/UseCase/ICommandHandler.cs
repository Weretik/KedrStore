namespace Application.Common.Abstractions.UseCase;

public interface ICommandHandler<TCommand, TResult>
    : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>;
