﻿namespace KittySaver.Shared.Hateoas;

public sealed record Link(string Href, string Rel, string Method, bool Templated = false);