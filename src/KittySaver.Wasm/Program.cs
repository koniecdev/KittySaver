using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KittySaver.Wasm;
using KittySaver.Wasm.Shared.HttpClients;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    {"ApiBaseUrl", "https://localhost:7127/"}
}!);
builder.Services.AddApiClient(builder.Configuration);

await builder.Build().RunAsync();