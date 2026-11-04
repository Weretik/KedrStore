using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Reorder;

public sealed class ReorderHomeFaqCommandValidator : AbstractValidator<ReorderHomeFaqCommand>
{
    public ReorderHomeFaqCommandValidator() => RuleFor(x => x.Ids).SetValidator(new HomeOrderValidator());
}
