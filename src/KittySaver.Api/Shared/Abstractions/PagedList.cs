namespace KittySaver.Api.Shared.Abstractions;

public sealed class PagedList<TResponse>
{
    public required ICollection<TResponse> Items { get; init; }
    public required int Total { get; init; }
    public ICollection<Link> Links { get; init; } = new List<Link>();
}