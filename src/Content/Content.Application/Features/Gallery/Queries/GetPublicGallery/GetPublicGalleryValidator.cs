namespace Content.Application.Features.Gallery.Queries.GetPublicGallery;

public sealed class GetPublicGalleryValidator : AbstractValidator<GetPublicGalleryQuery>
{
    public GetPublicGalleryValidator() =>
        RuleFor(x => x.Language).Must(language => language is "uk" or "ru")
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
