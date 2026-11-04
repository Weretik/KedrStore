namespace Media.Application.Features.Common;

internal static class MediaResults
{
    /// <summary>Carries a failed result over to another value type with the same status and errors.</summary>
    public static Result<T> Fail<T>(IResult failure) => failure.Status switch
    {
        ResultStatus.Invalid => Result<T>.Invalid(failure.ValidationErrors),
        ResultStatus.Conflict => Result<T>.Conflict(failure.Errors.ToArray()),
        ResultStatus.NotFound => Result<T>.NotFound(failure.Errors.ToArray()),
        ResultStatus.Unavailable => Result<T>.Unavailable(failure.Errors.ToArray()),
        _ => Result<T>.Error(new ErrorList(failure.Errors))
    };
}
