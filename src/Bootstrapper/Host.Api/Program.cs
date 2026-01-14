Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();

await app.RunStartupTasksAsync();

app.UseHostPipeline();
app.AddRecurringJobs();

app.Run();

