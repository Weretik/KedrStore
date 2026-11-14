using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.CreateCatalogAttribute;

/// <summary>Admin adds an attribute of their own; it is manual from the start, so the dictionary import leaves it.</summary>
public sealed record CreateCatalogAttributeCommand(CreateCatalogAttributeRequest Request)
    : ICommand<Result<AdminCatalogAttributeDto>>
{
    public const string YesNo = "yesno";

    /// <summary><c>yesno</c> is a list with the values «Так» and «Ні» created at once.</summary>
    public static readonly IReadOnlyList<string> Kinds = ["enum", "number", YesNo];
}
