namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public sealed class PagedList<TResponse>
{
    public required ICollection<TResponse> Items { get; init; }
    public required int Total { get; init; }
    public required ICollection<Link> Links { get; init; }
}