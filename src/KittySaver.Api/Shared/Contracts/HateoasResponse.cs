using System.Collections;
using KittySaver.Api.Shared.Abstractions;

namespace KittySaver.Api.Shared.Contracts;

public class HateoasResponse : IHateoasResponse
{
    public ICollection<Link> Links { get; set; } = new List<Link>();
}

public class PersonHateoasResponse(Guid id) : IHateoasPersonResponse
{
    public Guid Id { get; } = id;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}
public class CatHateoasResponse(Guid id, Guid personId, Guid? advertisementId) : IHateoasCatResponse
{
    public Guid Id { get; } = id;
    public Guid PersonId { get; } = personId;
    public Guid? AdvertisementId { get; } = advertisementId;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}