using ImageEditorWeb.Client;
using ImageEditorWeb.Client.Services;
using ImageEditorWeb.Core.Services;
using ImageEditorWeb.Core.Tools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<LayerService>();
builder.Services.AddScoped<ToolManager>();
builder.Services.AddScoped<CanvasManager>();
await builder.Build().RunAsync();
