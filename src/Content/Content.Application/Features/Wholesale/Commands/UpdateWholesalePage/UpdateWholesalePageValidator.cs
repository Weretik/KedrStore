using Content.Application.Features.Wholesale.Shared;

namespace Content.Application.Features.Wholesale.Commands.UpdateWholesalePage;

public sealed class UpdateWholesalePageValidator : AbstractValidator<UpdateWholesalePageCommand>
{
    public UpdateWholesalePageValidator()
    {
        RuleFor(x => x.Texts).NotNull()
            .Must(texts => texts.Count == WholesalePage.Languages.Count
                           && WholesalePage.Languages.All(language => texts.Count(text => text?.Language == language) == 1))
            .WithMessage("Send exactly one 'uk' and one 'ru' text.").OverridePropertyName("texts");
        RuleForEach(x => x.Texts).SetValidator(new WholesaleTextValidator()).When(x => x.Texts is not null)
            .OverridePropertyName("texts");
    }
}
