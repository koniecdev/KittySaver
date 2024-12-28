using KittySaver.Api.Shared.Infrastructure.ApiComponents;

namespace KittySaver.Api.Shared.Abstractions;

public sealed class Link(string href, string rel, string method, bool templated = false)
{
    public string Href { get; } = href;
    public string Rel { get; } = rel;
    public string Method { get; } = method;
    public bool Templated { get; } = templated;
}

public interface ILinkService
{
    Link Generate(EndpointInfo endpointInfo, object? routeValues = null, bool isSelf = false, bool isTemplated = false);
    Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET", bool isTemplated = false);
    List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit);
}

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : ILinkService
{
    public Link Generate(EndpointInfo endpointInfo, object? routeValues = null, bool isSelf = false, bool isTemplated = false)
    {
        string href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointInfo.EndpointName, routeValues)!;
        Link link = new Link(
            href: href,
            rel: isSelf ? EndpointNames.SelfRel : endpointInfo.Rel,
            method: endpointInfo.Verb,
            templated: isTemplated);
        return link;
    }

    public Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET", bool isTemplated = false)
    {
        string href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointName, routeValues)!;
        Link link = new Link(
            href: href,
            rel: rel,
            method: verb,
            templated: isTemplated);
        return link;
    }

    public List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit)
    {
        List<Link> links = [];
        string href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointName)!;
        if (limit is not null)
        {
            links.Add(new Link(
                href: $"{href}?offset={{offset}}&limit={limit}",
                rel: "by-offset",
                method: "GET",
                templated: true));
        }

        if (offset is not null)
        {
            links.Add(new Link(
                href: $"{href}?offset={offset}&limit={{limit}}",
                rel: "by-limit",
                method: "GET",
                templated: true));
        }
        
        links.Add(new Link(
            href: $"{href}?offset={{offset}}&limit={{limit}}",
            rel: "by-page",
            method: "GET",
            templated: true));
        return links;
    }
}