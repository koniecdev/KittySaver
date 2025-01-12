using KittySaver.Wasm.Shared.HttpClients.MainApiResponses;
using Microsoft.AspNetCore.Components;

namespace KittySaver.Wasm.Shared.Components;

public abstract class ApiAwareComponentBase : ComponentBase
{
    [Inject] protected IApiNavigationService ApiNavigation { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await ApiNavigation.InitializeAsync();
        await base.OnInitializedAsync();
    }
}