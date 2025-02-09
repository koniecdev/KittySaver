using KittySaver.Shared.Common;

namespace KittySaver.Shared.Hateoas;

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
    public bool IsThumbnailUploaded { get; }
}

public interface IHateoasAdvertisementResponse : IHateoasResponse
{
    public Guid Id { get; }
    public Guid PersonId { get; }
    public AdvertisementStatus Status { get; }
}

public class PersonHateoasResponse(Guid id) 
    : IHateoasPersonResponse
{
    public Guid Id { get; } = id;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}
public class CatHateoasResponse(Guid id, Guid personId, Guid? advertisementId, bool isThumbnailUploaded)
    : IHateoasCatResponse
{
    public Guid Id { get; } = id;
    public Guid PersonId { get; } = personId;
    public Guid? AdvertisementId { get; } = advertisementId;
    public bool IsThumbnailUploaded { get; } = isThumbnailUploaded;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}
public class AdvertisementHateoasResponse(Guid id, Guid personId, AdvertisementStatus status)
    : IHateoasAdvertisementResponse
{
    public Guid Id { get; } = id;
    public Guid PersonId { get; } = personId;
    public AdvertisementStatus Status { get; } = status;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}