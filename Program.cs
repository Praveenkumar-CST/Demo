
using Blazored.SessionStorage;
using FingerFrontend.AdminAttendanceViewModel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using WiseHR;
using WiseHR.Services;
using Syncfusion.Blazor;
using WiseHR.Models.NewFolder;
using WiseHR.Services.ChatFormatting;

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

builder.Services.AddScoped<BankIFSCService>();

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

builder.Services.AddSyncfusionBlazor();

builder.Services.AddScoped<IUserService, UserService>();
var apiBaseUrl = "https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net";
//var apiBaseUrl = "https://localhost:7021/";

// Register ChatService
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();

builder.Services.AddScoped<IChatMessageFormatter, ChatMessageFormatter>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddSingleton(new ApiConfig { BaseUrl = apiBaseUrl });

await builder.Build().RunAsync();