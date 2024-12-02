using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.Repositories;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.Events;

public class PersonDeletedDomainEventHandler(IPersonRepository personRepository) : INotificationHandler<PersonDeletedDomainEvent>
{
    public async Task Handle(PersonDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        await personRepository.RemoveAllPersonAdvertisementsAsync(notification.PersonId, cancellationToken);
    }
}