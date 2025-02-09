global using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KittySaver.Wasm;
using KittySaver.Wasm.Shared.Auth;
using KittySaver.Wasm.Shared.Components;
using KittySaver.Wasm.Shared.HttpClients;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<IApiNavigationService, ApiNavigationService>();
builder.Services.AddScoped<IAdvertisementStateService, AdvertisementStateService>();
builder.Services.AddAuthorizationCore(); 
builder.Services.AddApiClient();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();