namespace Media.Application.Features.Files.Queries.GetMediaFile;

public sealed class GetMediaFileValidator : AbstractValidator<GetMediaFileQuery>
{
    public GetMediaFileValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
