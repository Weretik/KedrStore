using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Update;

public sealed class UpdateHomeFaqItemCommandValidator : AbstractValidator<UpdateHomeFaqItemCommand>
{
    public UpdateHomeFaqItemCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Texts).NotNull().SetValidator(new HomeFaqInputValidator());
    }
}
