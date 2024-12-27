using KittySaver.Api.Shared.Infrastructure.ApiComponents;

namespace KittySaver.Api.Shared.Abstractions;

public sealed class Link(string href, string rel, string method)
{
    public string Href { get; } = href;
    public string Rel { get; } = rel;
    public string Method { get; } = method;
}

public interface ILinkService
{
    Link Generate(EndpointInfo endpointInfo, object? routeValues = null, bool isSelf = false);
}

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : ILinkService
{
    public Link Generate(EndpointInfo endpointInfo, object? routeValues = null, bool isSelf = false)
    {
        string href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointInfo.EndpointName, routeValues)!;
        Link link = new Link(
            href: href,
            rel: isSelf ? EndpointNames.SelfRel : endpointInfo.Rel,
            method: endpointInfo.Verb);
        return link;
    }
}