using KittySaver.Domain.Common.Primitives;

namespace KittySaver.Domain.Persons.Events;

public record AssignedToAdvertisementCatStatusChangedDomainEvent(Guid AdvertisementId) : DomainEvent;
