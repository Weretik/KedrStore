using Microsoft.Extensions.Configuration;

namespace Host.Jobs.Tasks.Catalog;

/// <summary>The three 1C catalogue roots the site sells from: doors, hardware, Cosmos.</summary>
internal static class OneCRoots
{
    public static IReadOnlyList<string> Of(IServiceProvider services)
    {
        var configuration = (IConfiguration)services.GetService(typeof(IConfiguration))!;
        string[] keys = ["OneC:DoorsRootCategoryId", "OneC:HardwareRootCategoryId", "OneC:CosmosRootCategoryId"];
        return keys
            .Select(key => configuration[key] ?? throw new InvalidOperationException($"Немає налаштування {key}."))
            .ToArray();
    }
}
