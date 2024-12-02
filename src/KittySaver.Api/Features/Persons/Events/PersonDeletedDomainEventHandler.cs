using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.Events;
using MediatR;

namespace KittySaver.Api.Features.Persons.Events;

public class PersonDeletedDomainEventHandler(IPersonRepository personRepository) : INotificationHandler<PersonDeletedDomainEvent>
{
    public async Task Handle(PersonDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        await personRepository.RemoveAllPersonAdvertisementsAsync(notification.PersonId, cancellationToken);
    }
}