using BlazorApp4;
using BlazorApp4.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;

// --------------------------------------------------------
// Program.cs
// Entry point for the Blazor WebAssembly application.
// --------------------------------------------------------
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// --------------------------------------------------------
// Root Components
// --------------------------------------------------------
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --------------------------------------------------------
// Dependency Injection (Services)
// --------------------------------------------------------
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IStorageService, LokalStorageService>();
builder.Services.AddSingleton<PinLockService>();
// --------------------------------------------------------
// HttpClient Configuration
// --------------------------------------------------------
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// --------------------------------------------------------
// Logging Configuration
// --------------------------------------------------------
// In Blazor WebAssembly, logs are written to the browser console.
// Adjust log levels as needed for debugging or production.
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

// --------------------------------------------------------
// Build and Run Application
// --------------------------------------------------------
await builder.Build().RunAsync();

