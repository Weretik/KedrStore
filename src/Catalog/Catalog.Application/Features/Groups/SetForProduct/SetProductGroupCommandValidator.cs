using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.SetForProduct;

public sealed class SetProductGroupCommandValidator : AbstractValidator<SetProductGroupCommand>
{
    public SetProductGroupCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.GroupId).GreaterThan(0)
            .When(command => command.Request?.GroupId is not null).OverridePropertyName("groupId");
        RuleFor(command => command.Request.GroupId).Null()
            .WithMessage("Automatic grouping does not take a group.")
            .When(command => command.Request is { Automatic: true }).OverridePropertyName("groupId");
    }
}
