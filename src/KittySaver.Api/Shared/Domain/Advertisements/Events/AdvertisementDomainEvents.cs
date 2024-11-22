using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.Advertisement.Events;

public record AdvertisementClosedDomainEvent(Guid AdvertisementId) : DomainEvent;