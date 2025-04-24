using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
<<<<<<< HEAD
using WiseHR;
using WiseHR.Services;
using MudBlazor.Services;
using MudBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//  Root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//  Base address for API requests
var backendBaseUrl = "http://localhost:5243/"; // 👈 Update if backend URL changes

//  MudBlazor config
=======
using WiseHR.Services;
using WiseHR;
using MudBlazor.Services;
using MudBlazor;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Use HTTPS endpoint for the backend
builder.Services.AddBlazoredSessionStorage();

>>>>>>> d8524e0ce06483b23a43a0effce5d37f9631f59e
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.HideTransitionDuration = 100;
    config.SnackbarConfiguration.ShowTransitionDuration = 100;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});

<<<<<<< HEAD
//  Register services
=======
>>>>>>> d8524e0ce06483b23a43a0effce5d37f9631f59e
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<BankingService>();
builder.Services.AddScoped<ExperienceService>();
<<<<<<< HEAD
builder.Services.AddScoped<IUserService, UserService>();

// HttpClient with backend base URI
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(backendBaseUrl)
});
=======
builder.Services.AddScoped<CountryService>();


builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7021/") });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://172.210.14.62:5000/") });
>>>>>>> d8524e0ce06483b23a43a0effce5d37f9631f59e

await builder.Build().RunAsync();
