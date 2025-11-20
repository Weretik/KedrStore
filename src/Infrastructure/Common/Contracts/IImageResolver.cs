namespace Infrastructure.Common.Contracts;

public interface IImageResolver
{
    Task<string> ResolveAsync(int id, CancellationToken ct);
}
