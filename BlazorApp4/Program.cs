using BlazorApp4;
using BlazorApp4.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//Register services for DI
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IStorageService, LokalStorageService>();
builder.Services.AddSingleton<PinLockService>();

// Configure logging for Blazor WebAssembly ".NET8 "
// The Browser Console provider is enabled by default in WebAssembly.
// Adjust levels/filters as needed.
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
