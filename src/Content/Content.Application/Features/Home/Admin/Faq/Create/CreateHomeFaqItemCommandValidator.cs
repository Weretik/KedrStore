using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Create;

public sealed class CreateHomeFaqItemCommandValidator : AbstractValidator<CreateHomeFaqItemCommand>
{
    public CreateHomeFaqItemCommandValidator() =>
        RuleFor(x => x.Texts).NotNull().SetValidator(new HomeFaqInputValidator());
}
