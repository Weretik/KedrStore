namespace Application.Common.Errors;

public sealed record AppError(string Code, string Description)
{
    public string? Details { get; private init; }

    public AppError WithDetails(string details) =>
        this with { Details = details };

    public override string ToString() =>
        $"{Code}: {Description}" + (Details is not null ? $" | Details: {Details}" : string.Empty);

}
