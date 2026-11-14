using Identity.Application.Features.Auth.Shared;
using Identity.Application.Features.Auth.Queries.SessionMe;

namespace Identity.Application.Features.Auth.Queries.SessionMe;

public sealed record SessionMeQuery : IQuery<Result<SessionMeResult>>;
