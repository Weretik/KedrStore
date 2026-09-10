namespace IntegrationTests.Platform.Api;

public sealed class ManagerOrderReadContractTests
{
    [Fact]
    public void VersionedAndAggregateContractsDeclareManagerOrderReads()
    {
        var root = FindRepositoryRoot();
        var featureContract = File.ReadAllText(Path.Combine(
            root,
            "docs",
            "sdd",
            "contracts",
            "sales",
            "manager-order-read.openapi.yaml"));
        var aggregateContract = File.ReadAllText(Path.Combine(root, "docs", "sdd", "contracts", "openapi.yaml"));

        Assert.Contains("/api/admin/orders:", featureContract, StringComparison.Ordinal);
        Assert.Contains("operationId: getAdminOrders", featureContract, StringComparison.Ordinal);
        Assert.Contains("operationId: getAdminOrderById", featureContract, StringComparison.Ordinal);
        Assert.Contains("maximum: 100", featureContract, StringComparison.Ordinal);
        Assert.Contains("bearerAuth: []", featureContract, StringComparison.Ordinal);
        Assert.Contains("ManagerOrderPage:", featureContract, StringComparison.Ordinal);
        Assert.Contains("ManagerOrderDetail:", featureContract, StringComparison.Ordinal);
        Assert.Contains("type: [string, 'null']", featureContract, StringComparison.Ordinal);
        Assert.Contains("format: date-time", featureContract, StringComparison.Ordinal);
        Assert.Contains("multipleOf: 0.01", featureContract, StringComparison.Ordinal);
        Assert.Contains("./sales/manager-order-read.openapi.yaml#/paths/~1api~1admin~1orders/get", aggregateContract,
            StringComparison.Ordinal);
        Assert.Contains("./sales/manager-order-read.openapi.yaml#/paths/~1api~1admin~1orders~1{orderId}", aggregateContract,
            StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
