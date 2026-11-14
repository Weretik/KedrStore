using Catalog.Domain.Groups;

namespace Catalog.Application.Features.Groups.Commands.CreateProductGroup;

public sealed class CreateProductGroupValidator : AbstractValidator<CreateProductGroupCommand>
{
    public CreateProductGroupValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.CategoryId).GreaterThan(0)
            .When(command => command.Request is not null).OverridePropertyName("categoryId");
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("name");
        RuleFor(command => command.Request.ProductIds)
            .NotNull()
            .Must(ids => ids.Count <= CreateProductGroupCommand.MaxProducts)
            .WithMessage($"У нову групу можна додати не більше {CreateProductGroupCommand.MaxProducts} товарів.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Ідентифікатори товарів мають бути додатні й без повторів.")
            .When(command => command.Request is not null).OverridePropertyName("productIds");
    }
}

/// <summary>A person creates a group; its products are pinned to it and automation never moves them.</summary>
