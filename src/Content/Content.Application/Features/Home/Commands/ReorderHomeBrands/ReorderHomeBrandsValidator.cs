using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.ReorderHomeBrands;

public sealed class ReorderHomeBrandsValidator : AbstractValidator<ReorderHomeBrandsCommand>
{
    public ReorderHomeBrandsValidator() => RuleFor(x => x.Ids).SetValidator(new HomeOrderValidator());
}
