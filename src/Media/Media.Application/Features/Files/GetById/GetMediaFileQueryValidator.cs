namespace Media.Application.Features.Files.GetById;

public sealed class GetMediaFileQueryValidator : AbstractValidator<GetMediaFileQuery>
{
    public GetMediaFileQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
