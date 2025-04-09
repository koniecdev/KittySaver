using KittySaver.Shared.Hateoas;

namespace KittySaver.Shared.Responses;

public interface IPagedList<TResponse>
{
    public ICollection<TResponse> Items { get; }

}
public sealed class PagedList<TResponse> : IPagedList<TResponse>
{
    public required ICollection<TResponse> Items { get; init; }
    public required int Total { get; init; }
    public required ICollection<Link> Links { get; init; }
}

