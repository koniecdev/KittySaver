using System.Collections;
using KittySaver.Api.Features.Advertisements.SharedContracts;
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
public class AdvertisementHateoasResponse(Guid id, Guid personId, AdvertisementResponse.AdvertisementStatus status) : IHateoasAdvertisementResponse
{
    public Guid Id { get; } = id;
    public Guid PersonId { get; } = personId;
    public AdvertisementResponse.AdvertisementStatus Status { get; } = status;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}