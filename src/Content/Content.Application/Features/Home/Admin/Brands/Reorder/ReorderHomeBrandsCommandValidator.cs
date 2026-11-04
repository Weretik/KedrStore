using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Reorder;

public sealed class ReorderHomeBrandsCommandValidator : AbstractValidator<ReorderHomeBrandsCommand>
{
    public ReorderHomeBrandsCommandValidator() => RuleFor(x => x.Ids).SetValidator(new HomeOrderValidator());
}
