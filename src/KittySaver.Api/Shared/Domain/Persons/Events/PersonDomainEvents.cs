using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.Persons.Events;

public record PersonDeletedDomainEvent(Guid PersonId) : DomainEvent;

