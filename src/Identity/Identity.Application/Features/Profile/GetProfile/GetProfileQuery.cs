using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.GetProfile;

public sealed record GetProfileQuery : IQuery<Result<ProfileDto>>;
