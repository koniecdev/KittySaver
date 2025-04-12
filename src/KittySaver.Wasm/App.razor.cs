using Microsoft.AspNetCore.Components;

namespace KittySaver.Wasm;

public partial class App
{
    [Inject]
    private IApiUrlProvider ApiUrlProvider { get; set; } = null!;

    protected override void OnInitialized()
    {
        EnvironmentConfiguration.Initialize(ApiUrlProvider);
        base.OnInitialized();
    }
}