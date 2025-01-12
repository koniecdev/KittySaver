using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KittySaver.Wasm;
using KittySaver.Wasm.Shared.HttpClients;
using KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<IApiNavigationService, ApiNavigationService>();
builder.Services.AddApiClient(builder.Configuration);

await builder.Build().RunAsync();