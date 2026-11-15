using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Recognises a broken unique index, the guard of every "create once" (database rules 4): the caller catches it and
/// answers with the row that won instead of a 500.
/// </summary>
public static class UniqueViolation
{
    public static bool Is(DbUpdateException error)
        => error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
