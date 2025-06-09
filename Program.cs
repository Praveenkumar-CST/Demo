using Blazored.SessionStorage;
using FingerFrontend.AdminAttendanceViewModel;
using FingerFrontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using WiseHR;
using WiseHR.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<AssetStateService>();
builder.Services.AddScoped<AssetsService>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.HideTransitionDuration = 100;
    config.SnackbarConfiguration.ShowTransitionDuration = 100;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});


builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpClient<EmployeeService>();
builder.Services.AddScoped<EmployeeService>();

builder.Services.AddHttpClient<BankingService>();
builder.Services.AddScoped<BankingService>();

builder.Services.AddHttpClient<ExperienceService>();
builder.Services.AddScoped<ExperienceService>();

builder.Services.AddHttpClient<ReportService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<AnalyticsCacheService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHttpClient<MentorAssignmentService>();
builder.Services.AddScoped<MentorAssignmentService>();

builder.Services.AddScoped<CountryService>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddScoped<LeaveManagementService>();





builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7021/") });


await builder.Build().RunAsync();