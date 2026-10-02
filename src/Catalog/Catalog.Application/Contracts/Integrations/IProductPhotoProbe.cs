using Catalog.Domain.Enums;

namespace Catalog.Application.Contracts.Integrations;

public interface IProductPhotoProbe
{
    Task<ProductPhotoProbeResult> ProbeAsync(string photoUrl, CancellationToken cancellationToken);
}

public sealed record ProductPhotoProbeResult(
    ProductPhotoStatus Status,
    int? HttpStatusCode,
    string? DiagnosticCode,
    string? DiagnosticMessage);
