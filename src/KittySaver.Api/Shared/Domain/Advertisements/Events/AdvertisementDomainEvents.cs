using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.Advertisements.Events;

public record AdvertisementDeletedDomainEvent(Guid AdvertisementId, Guid OwnerPersonId) : DomainEvent;
public record AdvertisementClosedDomainEvent(Guid AdvertisementId) : DomainEvent;
