namespace KittySaver.Api.Shared.Abstractions;

public sealed class Link(string href, string rel, string method, bool templated = false)
{
    public string Href { get; } = href;
    public string Rel { get; } = rel;
    public string Method { get; } = method;
    public bool Templated { get; } = templated;
}