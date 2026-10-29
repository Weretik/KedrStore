using Identity.Application.Features.Profile.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Profile.GetProfile;

public sealed class GetProfileQueryHandler(IProfileService profileService)
    : IQueryHandler<GetProfileQuery, Result<ProfileDto>>
{
    public async ValueTask<Result<ProfileDto>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
        => await profileService.GetAsync(cancellationToken);
}
