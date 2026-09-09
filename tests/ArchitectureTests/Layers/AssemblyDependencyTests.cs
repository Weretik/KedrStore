using System.Reflection;
using BuildingBlocks.Domain.Abstractions;
using NetArchTest.Rules;

namespace ArchitectureTests.Layers;

public sealed class AssemblyDependencyTests
{
    private static readonly Assembly[] DomainAssemblies =
    [
        typeof(IAggregateRoot).Assembly,
        typeof(Catalog.Domain.CatalogDomainAssemblyMarker).Assembly,
        typeof(Identity.Domain.IdentityDomainAssemblyMarker).Assembly,
        typeof(Sales.Domain.SalesDomainAssemblyMarker).Assembly
    ];

    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(BuildingBlocks.Application.ApplicationAssemblyMarker).Assembly,
        typeof(Catalog.Application.CatalogApplicationAssemblyMarker).Assembly,
        typeof(Identity.Application.IdentityApplicationAssemblyMarker).Assembly,
        typeof(Sales.Application.SalesApplicationAssemblyMarker).Assembly
    ];

    [Fact]
    public void DomainTypes_DoNotDependOnOuterLayersOrFrameworkAdapters()
    {
        var result = Types.InAssemblies(DomainAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BuildingBlocks.Application",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.Extensions",
                "Catalog.Application",
                "Catalog.Infrastructure",
                "Catalog.Api",
                "Identity.Application",
                "Identity.Infrastructure",
                "Identity.Api",
                "Sales.Application",
                "Sales.Infrastructure",
                "Sales.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result.FailingTypeNames));
    }

    [Fact]
    public void ApplicationTypes_DoNotDependOnInfrastructureApiOrHosts()
    {
        var result = Types.InAssemblies(ApplicationAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Catalog.Infrastructure",
                "Catalog.Api",
                "Identity.Infrastructure",
                "Identity.Api",
                "Sales.Infrastructure",
                "Sales.Api",
                "Host.Api",
                "Host.Jobs")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result.FailingTypeNames));
    }

    private static string FormatFailures(IEnumerable<string>? failingTypeNames)
        => "Forbidden dependencies found: " + string.Join(", ", failingTypeNames ?? []);
}
