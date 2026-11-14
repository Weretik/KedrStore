using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.Commands.SetProductGroupMain;

public sealed class SetProductGroupMainValidator : AbstractValidator<SetProductGroupMainCommand>
{
    public SetProductGroupMainValidator()
    {
        RuleFor(command => command.GroupId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ProductId).GreaterThan(0)
            .When(command => command.Request is not null).OverridePropertyName("productId");
    }
}
