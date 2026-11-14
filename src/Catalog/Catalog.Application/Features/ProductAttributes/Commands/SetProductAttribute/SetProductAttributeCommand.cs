using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.SetProductAttribute;

/// <summary>
/// A person sets the values of one attribute of a product. They become manual and locked: the import, the parser, and
/// the 1C sync no longer change them until «Повернути автоматичне».
/// </summary>
public sealed record SetProductAttributeCommand(int ProductId, int AttributeId, SetProductAttributeRequest Request)
    : ICommand<Result<AdminProductAttributesDto>>;
