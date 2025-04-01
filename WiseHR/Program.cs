using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WiseHR.Services;
using WiseHR;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Use HTTPS endpoint for the backend

builder.Services.AddMudServices();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7021/") });
await builder.Build().RunAsync();
