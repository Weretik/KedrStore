using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Create;

/// <summary>Admin adds an attribute of their own; it is manual from the start, so the dictionary import leaves it.</summary>
public sealed record CreateCatalogAttributeCommand(CreateCatalogAttributeRequest Request)
    : ICommand<Result<AdminCatalogAttributeDto>>
{
    public static readonly IReadOnlyList<string> Kinds = ["enum", "number"];
}
