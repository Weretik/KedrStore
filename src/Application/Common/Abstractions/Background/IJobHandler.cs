namespace Application.Common.Abstractions.Background;

public interface IJobHandler
{
    Task Execute(object args, CancellationToken cancellationToken = default);
}
