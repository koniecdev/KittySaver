using KittySaver.Shared.Hateoas;

namespace KittySaver.Shared.Responses;

public sealed class GetApiDiscoveryV1Response : IHateoasResponse
{
    public Guid? PersonId { get; set; }
    public ICollection<Link> Links { get; set; } = new List<Link>();
}