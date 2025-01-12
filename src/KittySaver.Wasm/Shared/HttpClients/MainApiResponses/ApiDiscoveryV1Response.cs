namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public sealed class GetApiDiscoveryV1Response
{
    public IReadOnlyCollection<Link> Links { get; set; } = new List<Link>();
}