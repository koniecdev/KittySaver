using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Shared.Responses;

public sealed class GetApiDiscoveryV1Response : IHateoasResponse
{
    public PersonId? PersonId { get; set; }
    public ICollection<Link> Links { get; set; } = new List<Link>();
}