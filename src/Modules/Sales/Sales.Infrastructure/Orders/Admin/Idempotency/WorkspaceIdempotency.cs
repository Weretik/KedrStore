using System.Security.Cryptography;
using System.Text;

namespace Sales.Infrastructure.Orders;

internal static class WorkspaceIdempotency
{
    internal static string Hash(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
