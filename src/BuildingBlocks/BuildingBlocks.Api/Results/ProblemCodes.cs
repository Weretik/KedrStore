using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Api.Results;

/// <summary>
/// The machine-readable <c>code</c> of every error answer. A validator's own error code wins on 400; otherwise the
/// code follows the HTTP status.
/// </summary>
public static class ProblemCodes
{
    public const string Validation = "validation";
    public const string InvalidRequest = "invalid_request";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string Unavailable = "unavailable";
    public const string Unexpected = "unexpected";

    /// <summary>The code of an error answer that carries no code of its own (binding failure, 401/403, 500).</summary>
    public static string ForStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => InvalidRequest,
        StatusCodes.Status401Unauthorized => Unauthorized,
        StatusCodes.Status403Forbidden => Forbidden,
        StatusCodes.Status404NotFound => NotFound,
        StatusCodes.Status409Conflict => Conflict,
        StatusCodes.Status503ServiceUnavailable => Unavailable,
        >= 500 => Unexpected,
        _ => InvalidRequest,
    };
}
