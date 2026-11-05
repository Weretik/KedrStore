using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Restore;

/// <summary>
/// «Повернути автоматичне»: drops the manual values of one attribute and brings back the automatic ones kept for them.
/// Positions automation never had disappear; new automatic positions come with the next recognition run.
/// </summary>
public sealed record RestoreProductAttributeCommand(int ProductId, int AttributeId)
    : ICommand<Result<AdminProductAttributesDto>>;
