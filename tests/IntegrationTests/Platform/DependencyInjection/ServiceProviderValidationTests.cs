extern alias JobsHost;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JobsHostServicesExtensions = JobsHost::Host.Jobs.JobsHostServicesExtensions;
using JobsServiceProviderOptions = JobsHost::Host.Jobs.JobsServiceProviderOptions;

namespace IntegrationTests.Platform.DependencyInjection;

/// <summary>
/// Both hosts validate the container in every environment: a Singleton that holds a Scoped service (a captive
/// DbContext) or a missing registration fails at startup, not on the first request in production. No database is
/// opened; building the container does not connect.
/// </summary>
public sealed class ServiceProviderValidationTests
{
    [Fact]
    public void ApiHost_BuildsWithScopeAndBuildValidationInProduction()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=validation;Username=x;Password=x");
        });

        var exception = Record.Exception(() => factory.Services.GetRequiredService<IServiceScopeFactory>());

        Assert.Null(exception);
    }

    [Fact]
    public void JobsHost_UsesScopeAndBuildValidation()
    {
        Assert.True(JobsServiceProviderOptions.Value.ValidateScopes);
        Assert.True(JobsServiceProviderOptions.Value.ValidateOnBuild);
    }

    [Fact]
    public void JobsHost_ContainerPassesScopeAndBuildValidation()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Database=validation;Username=x;Password=x",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        JobsHostServicesExtensions.AddJobsHostServices(services, configuration);

        var exception = Record.Exception(() =>
        {
            using var provider = services.BuildServiceProvider(JobsServiceProviderOptions.Value);
        });

        Assert.Null(exception);
    }
}
