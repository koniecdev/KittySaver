using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Domain.Persons;

namespace KittySaver.Api.Shared.Hateoas;

public interface IHateoasResponse
{
    public ICollection<Link> Links { get; set; }
}

public interface IHateoasPersonResponse : IHateoasResponse
{
    public Guid Id { get; }
}

public interface IHateoasCatResponse : IHateoasResponse
{
    public Guid Id { get; }
    public Guid PersonId { get; }
    public Guid? AdvertisementId { get; }
}

public interface IHateoasAdvertisementResponse : IHateoasResponse
{
    public Guid Id { get; }
    public Guid PersonId { get; }
    public AdvertisementResponse.AdvertisementStatus Status { get; }
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
public class AdvertisementHateoasResponse(Guid id, Guid personId, AdvertisementResponse.AdvertisementStatus status)
    : IHateoasAdvertisementResponse
{
    public AdvertisementHateoasResponse(Guid id, Guid personId, Advertisement.AdvertisementStatus status) 
        : this(id, personId, (AdvertisementResponse.AdvertisementStatus)status)
    {
    }

    public Guid Id { get; } = id;
    public Guid PersonId { get; } = personId;
    public AdvertisementResponse.AdvertisementStatus Status { get; } = status;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}