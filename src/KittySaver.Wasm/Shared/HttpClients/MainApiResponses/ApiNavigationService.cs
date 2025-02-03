using System.Net.Http.Json;

namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public interface IApiNavigationService
{
    GetApiDiscoveryV1Response? Response { get; }
    Task InitializeAsync();
    Task ReInitializeAsync();
    Link? GetLink(string rel);
}

public class ApiNavigationService(IApiClient apiClient) : IApiNavigationService
{
    public GetApiDiscoveryV1Response? Response { get; private set; }

    public async Task InitializeAsync()
    {
        Response ??= await apiClient.GetAsync<GetApiDiscoveryV1Response>("https://localhost:7127/api/v1/");
    }
    
    public async Task ReInitializeAsync()
    {
        Response = await apiClient.GetAsync<GetApiDiscoveryV1Response>("https://localhost:7127/api/v1/");
    }

    public Link? GetLink(string rel) => Response?.Links.FirstOrDefault(x => x.Rel == rel);
}