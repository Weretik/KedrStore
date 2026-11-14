using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.ReorderHomeFaq;

public sealed class ReorderHomeFaqValidator : AbstractValidator<ReorderHomeFaqCommand>
{
    public ReorderHomeFaqValidator() => RuleFor(x => x.Ids).SetValidator(new HomeOrderValidator());
}
