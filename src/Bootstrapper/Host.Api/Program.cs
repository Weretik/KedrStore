var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();

await app.RunStartupTasksAsync();

app.UseHostPipeline();

app.Run();

