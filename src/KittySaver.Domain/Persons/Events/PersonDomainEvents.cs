using KittySaver.Domain.Common.Primitives;

namespace KittySaver.Domain.Persons.Events;

public record PersonDeletedDomainEvent(Guid PersonId) : DomainEvent;

