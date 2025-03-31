using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;

namespace KittySaver.Wasm.Shared.HttpClients;

public interface IApiNavigationService
{
    GetApiDiscoveryV1Response? Response { get; }
    Task InitializeAsync();
    Task RefreshAsync();
    Link? GetLink(string rel);
}

public class ApiNavigationService(IApiClient apiClient) : IApiNavigationService
{
    
    public GetApiDiscoveryV1Response? Response { get; private set; }

    public async Task InitializeAsync()
    {
        Response ??= await apiClient.GetAsync<GetApiDiscoveryV1Response>(StaticDetails.ApiUrl);
    }
    
    public async Task RefreshAsync()
    {
        Response = await apiClient.GetAsync<GetApiDiscoveryV1Response>(StaticDetails.ApiUrl);
    }

    public Link? GetLink(string rel) => Response?.Links.FirstOrDefault(x => x.Rel == rel);
}