using Catalog.Domain.Entities.Groups;

namespace Catalog.Application.Features.Groups.Create;

public sealed class CreateProductGroupCommandValidator : AbstractValidator<CreateProductGroupCommand>
{
    public CreateProductGroupCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.CategoryId).GreaterThan(0)
            .When(command => command.Request is not null).OverridePropertyName("categoryId");
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("name");
        RuleFor(command => command.Request.ProductIds)
            .NotNull()
            .Must(ids => ids.Count <= CreateProductGroupCommand.MaxProducts)
            .WithMessage($"A new group can take at most {CreateProductGroupCommand.MaxProducts} products.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Product IDs must be positive and distinct.")
            .When(command => command.Request is not null).OverridePropertyName("productIds");
    }
}

/// <summary>A person creates a group; its products are pinned to it and automation never moves them.</summary>
