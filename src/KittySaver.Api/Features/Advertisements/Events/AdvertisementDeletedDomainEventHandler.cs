using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements.Events;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements.Events;

public class AdvertisementDeletedDomainEventHandler(IPersonRepository personRepository)
    : INotificationHandler<AdvertisementDeletedDomainEvent>
{
    public async Task Handle(AdvertisementDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Person person = await personRepository.GetPersonByIdAsync(notification.OwnerPersonId, cancellationToken);
        person.UnassignCatsFromRemovedAdvertisement(notification.AdvertisementId);
    }
}