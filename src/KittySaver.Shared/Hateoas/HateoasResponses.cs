using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Shared.Hateoas;

public interface IHateoasResponse
{
    public ICollection<Link> Links { get; set; }
}

public interface IHateoasPersonResponse : IHateoasResponse
{
    public PersonId Id { get; }
}

public interface IHateoasCatResponse : IHateoasResponse
{
    public CatId Id { get; }
    public PersonId PersonId { get; }
    public AdvertisementId? AdvertisementId { get; }
    public bool IsThumbnailUploaded { get; }
    public bool IsAdopted { get; }
}

public interface IHateoasAdvertisementResponse : IHateoasResponse
{
    public AdvertisementId Id { get; }
    public PersonId PersonId { get; }
    public AdvertisementStatus Status { get; }
}

public class PersonHateoasResponse(PersonId id) 
    : IHateoasPersonResponse
{
    public PersonId Id { get; } = id;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}
public class CatHateoasResponse(CatId id, PersonId personId, AdvertisementId? advertisementId, bool isThumbnailUploaded, bool isAdopted)
    : IHateoasCatResponse
{
    public CatId Id { get; } = id;
    public PersonId PersonId { get; } = personId;
    public AdvertisementId? AdvertisementId { get; } = advertisementId;
    public bool IsThumbnailUploaded { get; } = isThumbnailUploaded;
    public bool IsAdopted { get; } = isAdopted;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}
public class AdvertisementHateoasResponse(AdvertisementId id, PersonId personId, AdvertisementStatus status)
    : IHateoasAdvertisementResponse
{
    public AdvertisementId Id { get; } = id;
    public PersonId PersonId { get; } = personId;
    public AdvertisementStatus Status { get; } = status;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}