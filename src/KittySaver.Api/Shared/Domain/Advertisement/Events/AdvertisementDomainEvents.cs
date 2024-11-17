using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.Advertisement.Events;

public record AdvertisementCreatedDomainEvent(Guid AdvertisementId, Guid PersonId, List<Guid> IdsOfCatsToAssignToAdvertisement) : DomainEvent;
public record AdvertisementClosedDomainEvent(Guid AdvertisementId) : DomainEvent;