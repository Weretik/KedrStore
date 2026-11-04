namespace Content.Application.Features.Home.Public.GetBlocks;

public sealed class GetPublicHomeBlocksQueryValidator : AbstractValidator<GetPublicHomeBlocksQuery>
{
    public GetPublicHomeBlocksQueryValidator() =>
        RuleFor(x => x.Language).Must(language => language is "uk" or "ru")
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
