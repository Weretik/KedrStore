namespace Content.Application.Features.Gallery.Public.GetList;

public sealed class GetPublicGalleryQueryValidator : AbstractValidator<GetPublicGalleryQuery>
{
    public GetPublicGalleryQueryValidator() =>
        RuleFor(x => x.Language).Must(language => language is "uk" or "ru")
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
