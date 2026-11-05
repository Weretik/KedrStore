using Content.Application.Features.Wholesale.Common;

namespace Content.Application.Features.Wholesale.Admin.Update;

public sealed class UpdateWholesalePageCommandValidator : AbstractValidator<UpdateWholesalePageCommand>
{
    public UpdateWholesalePageCommandValidator()
    {
        RuleFor(x => x.Texts).NotNull()
            .Must(texts => texts.Count == WholesalePage.Languages.Count
                           && WholesalePage.Languages.All(language => texts.Count(text => text?.Language == language) == 1))
            .WithMessage("Send exactly one 'uk' and one 'ru' text.").OverridePropertyName("texts");
        RuleForEach(x => x.Texts).SetValidator(new WholesaleTextValidator()).When(x => x.Texts is not null)
            .OverridePropertyName("texts");
    }
}
