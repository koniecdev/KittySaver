namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public sealed record Link(string Href, string Rel, string Method, bool Templated = false);