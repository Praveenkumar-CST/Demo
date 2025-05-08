using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WiseHR;
using WiseHR.Services;
using MudBlazor.Services;
using MudBlazor;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredSessionStorage();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.HideTransitionDuration = 100;
    config.SnackbarConfiguration.ShowTransitionDuration = 100;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});


builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<BankingService>();
builder.Services.AddScoped<ExperienceService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<CountryService>();


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7021/") });
<<<<<<< HEAD
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net/") });
=======
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net") });
>>>>>>> 2336a7881d8675e0f5cef80c8bea954dadf056f6

await builder.Build().RunAsync();