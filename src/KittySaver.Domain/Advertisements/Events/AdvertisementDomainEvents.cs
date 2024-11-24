using KittySaver.Domain.Common.Primitives;

namespace KittySaver.Domain.Advertisements.Events;

public record AdvertisementDeletedDomainEvent(Guid AdvertisementId, Guid OwnerPersonId) : DomainEvent;
public record AdvertisementClosedDomainEvent(Guid AdvertisementId) : DomainEvent;
