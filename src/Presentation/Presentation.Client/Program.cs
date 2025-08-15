using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Presentation.Shared.Common;
using Presentation.Shared.States;
using MudBlazor.Services;
using Presentation.Shared.Common.Abstractions;
using Presentation.Shared.Common.States;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped<IState>(sp => sp.GetRequiredService<BurgerMenuState>());
builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<BurgerMenuState>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
