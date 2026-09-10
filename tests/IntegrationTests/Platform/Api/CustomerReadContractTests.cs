namespace IntegrationTests.Platform.Api;

public sealed class CustomerReadContractTests
{
    [Fact]
    public void VersionedAndAggregateContractsDeclareCustomerReads()
    {
        var root = FindRepositoryRoot();
        var featureContract = File.ReadAllText(Path.Combine(
            root,
            "docs",
            "sdd",
            "contracts",
            "sales",
            "customer-read.openapi.yaml"));
        var aggregateContract = File.ReadAllText(Path.Combine(root, "docs", "sdd", "contracts", "openapi.yaml"));

        Assert.Contains("/api/admin/customers:", featureContract, StringComparison.Ordinal);
        Assert.Contains("operationId: getAdminCustomers", featureContract, StringComparison.Ordinal);
        Assert.Contains("operationId: getAdminCustomerById", featureContract, StringComparison.Ordinal);
        Assert.Contains("maximum: 100", featureContract, StringComparison.Ordinal);
        Assert.DoesNotContain("bearerAuth", featureContract, StringComparison.Ordinal);
        Assert.Contains("security: []", featureContract, StringComparison.Ordinal);
        Assert.DoesNotContain("'401'", featureContract, StringComparison.Ordinal);
        Assert.DoesNotContain("'403'", featureContract, StringComparison.Ordinal);
        Assert.Contains("CustomerPage:", featureContract, StringComparison.Ordinal);
        Assert.Contains("CustomerDetail:", featureContract, StringComparison.Ordinal);
        Assert.Contains("type: [string, 'null']", featureContract, StringComparison.Ordinal);
        Assert.DoesNotContain("identityUserId", featureContract, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("orderId", featureContract, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("./sales/customer-read.openapi.yaml#/paths/~1api~1admin~1customers", aggregateContract,
            StringComparison.Ordinal);
        Assert.Contains("./sales/customer-read.openapi.yaml#/paths/~1api~1admin~1customers~1{counterpartyId}",
            aggregateContract,
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
