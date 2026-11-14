namespace Content.Application.Features.Home.Queries.GetPublicHomeBlocks;

public sealed class GetPublicHomeBlocksValidator : AbstractValidator<GetPublicHomeBlocksQuery>
{
    public GetPublicHomeBlocksValidator() =>
        RuleFor(x => x.Language).Must(language => language is "uk" or "ru")
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
