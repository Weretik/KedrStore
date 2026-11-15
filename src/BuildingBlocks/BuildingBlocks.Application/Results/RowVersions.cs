namespace BuildingBlocks.Application.Results;

/// <summary>
/// Optimistic locking of admin edit forms (database rules 4). The form reads a row's version (PostgreSQL
/// <c>xmin</c>, <c>0</c> while the row does not exist yet) and sends it back with the save. A version that no longer
/// matches answers 409 instead of overwriting someone else's change. A save without a version is not checked, so an
/// older client keeps working.
/// </summary>
public static class RowVersions
{
    public const string ChangedMessage = "Ці дані вже змінив інший користувач. Оновіть сторінку й внесіть зміни ще раз.";

    public static bool IsStale(uint? sent, uint current) => sent is { } version && version != current;

    public static Result<T> Changed<T>() => Result<T>.Conflict(ChangedMessage);
}
