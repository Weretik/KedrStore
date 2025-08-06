using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Presentation.Shared.Common;
using Presentation.Shared.States;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<BurgerMenuState>();
builder.Services.AddScoped<IState>(sp => sp.GetRequiredService<BurgerMenuState>());

builder.Services.AddScoped<StateContainer>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
