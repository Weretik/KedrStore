using System.Globalization;
using System.Linq.Expressions;
using Sales.Application.Abstractions.Orders;
using Sales.Domain.Orders.Pricing;
using Sales.Application.Abstractions.Workspace;

namespace Sales.Application.Features.OrderWorkspace.Shared;

/// <summary>
/// Input rules the order workspace shares between its validators. Each failure carries the workspace's public error
/// code, which the admin uses to choose what to show.
/// </summary>
public static class WorkspaceInputRules
{
    public const int MaxLines = 100;
    public const int CounterpartyIdMaxLength = 64;
    public const int SearchMaxLength = 100;
    public const int MaxPageSize = 100;

    public const string LinesMessage = "Оберіть клієнта і додайте від 1 до 100 різних товарів із кількістю від 1.";

    /// <summary>A catalogue product ID as the admin sends it: a positive number without signs or leading zeros.</summary>
    public static bool IsProductId(string? value) =>
        value is not null &&
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) &&
        id is >= 1 and <= 1_000_000_000 &&
        value == id.ToString(CultureInfo.InvariantCulture);

    public static bool IsCounterpartyId(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= CounterpartyIdMaxLength;

    /// <summary>Whole kopecks, at most <see cref="OrderLinePricing.MaxPriceAdjustment"/> either way.</summary>
    public static bool IsPriceAdjustment(decimal value) =>
        Math.Abs(value) <= OrderLinePricing.MaxPriceAdjustment && decimal.Round(value, 2) == value;

    public static bool AreValidLines(IReadOnlyList<WorkspaceInputLine>? lines) =>
        lines is { Count: >= 1 and <= MaxLines } &&
        lines.All(line => line is not null && line.Quantity >= 1 && IsProductId(line.ProductId)) &&
        lines.Select(line => line.ProductId).Distinct(StringComparer.Ordinal).Count() == lines.Count;

    /// <summary>The same check for lines already stored, such as a draft's.</summary>
    public static DomainResult CheckLines(string? counterpartyId, IReadOnlyList<WorkspaceInputLine>? lines) =>
        IsCounterpartyId(counterpartyId) && AreValidLines(lines) &&
        lines!.All(line => IsPriceAdjustment(line.PriceAdjustment))
            ? DomainResult.Success()
            : WorkspaceErrors.InvalidLines();

    /// <summary>A customer and 1-100 different products with whole quantities and kopeck adjustments.</summary>
    public static void AddLineRules<T>(this AbstractValidator<T> validator,
        Expression<Func<T, string?>> counterpartyId, Expression<Func<T, IReadOnlyList<WorkspaceInputLine>?>> lines)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.RuleFor(counterpartyId)
            .Must(IsCounterpartyId).WithErrorCode("invalid_lines").WithMessage(LinesMessage);
        validator.RuleFor(lines)
            .Must(AreValidLines).WithErrorCode("invalid_lines").WithMessage(LinesMessage);
        validator.RuleFor(lines)
            .Must(value => value is null || value.All(line => line is null || IsPriceAdjustment(line.PriceAdjustment)))
            .WithErrorCode("invalid_price_adjustment")
            .WithMessage("Знижка або націнка — сума в гривнях з точністю до копійки, не більше 1 000 000 ₴.");
    }

    /// <summary>The paging and filter rules of the order and draft lists.</summary>
    public static void AddListRules<T>(this AbstractValidator<T> validator,
        Expression<Func<T, string?>> searchTerm, Expression<Func<T, string?>> counterpartyId,
        Expression<Func<T, int>> page, Expression<Func<T, int>> pageSize)
    {
        ArgumentNullException.ThrowIfNull(validator);
        const string message = "Некоректна сторінка, пошук або фільтр клієнта.";
        validator.RuleFor(searchTerm).MaximumLength(SearchMaxLength).WithErrorCode("invalid_query").WithMessage(message);
        validator.RuleFor(counterpartyId).MaximumLength(CounterpartyIdMaxLength).WithErrorCode("invalid_query")
            .WithMessage(message);
        validator.RuleFor(page).GreaterThanOrEqualTo(1).WithErrorCode("invalid_query").WithMessage(message);
        validator.RuleFor(pageSize).InclusiveBetween(1, MaxPageSize).WithErrorCode("invalid_query").WithMessage(message);
    }

    /// <summary>Calendar dates <c>yyyy-MM-dd</c>, the start not after the end.</summary>
    public static void AddDateRangeRule<T>(this AbstractValidator<T> validator,
        Func<T, string?> createdFrom, Func<T, string?> createdTo)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.RuleFor(message => message)
            .Must(message => WorkspaceDateRange.Parse(createdFrom(message), createdTo(message)).IsSuccess)
            .OverridePropertyName("CreatedFrom")
            .WithErrorCode("invalid_date_range")
            .WithMessage(WorkspaceDateRange.InvalidMessage);
    }

    public static void AddSortRule<T>(this AbstractValidator<T> validator,
        Expression<Func<T, string>> sort, IReadOnlySet<string> allowed)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.RuleFor(sort).Must(value => value is not null && allowed.Contains(value))
            .WithErrorCode("invalid_sort").WithMessage("Непідтримуване сортування.");
    }
}
