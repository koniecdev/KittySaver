global using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KittySaver.Wasm;
using KittySaver.Wasm.Shared.Auth;
using KittySaver.Wasm.Shared.Components;
using KittySaver.Wasm.Shared.HttpClients;
using KittySaver.Wasm.Shared.Validation;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSingleton<IApiUrlProvider, ApiUrlProvider>();
builder.Services.AddScoped<IApiNavigationService, ApiNavigationService>();
builder.Services.AddScoped<IAdvertisementStateService, AdvertisementStateService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddAuthorizationCore(); 
builder.Services.AddApiClient();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices(config => {
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});

await builder.Build().RunAsync();