using Identity.Application.Features.Auth.SessionMe.DTOs;

namespace Identity.Application.Features.Auth.SessionMe;

public sealed record SessionMeQuery : IQuery<Result<SessionMeResult>>;
