namespace Infrastructure.Common.Services;

public class EnvironmentService(IWebHostEnvironment env) : IEnvironmentService
{
    public bool IsProduction() => env.IsProduction();
}
