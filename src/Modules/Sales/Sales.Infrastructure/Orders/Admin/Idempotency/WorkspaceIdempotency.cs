using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace Sales.Infrastructure.Orders;

internal static class WorkspaceIdempotency
{
    internal static bool IsUniqueViolation(DbUpdateException error)
        => error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    internal static string Hash(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
