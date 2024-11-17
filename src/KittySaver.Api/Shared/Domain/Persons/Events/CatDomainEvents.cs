using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.Persons.Events;

public record AssignedToAdvertisementCatStatusChangedDomainEvent(Guid AdvertisementId) : DomainEvent;
