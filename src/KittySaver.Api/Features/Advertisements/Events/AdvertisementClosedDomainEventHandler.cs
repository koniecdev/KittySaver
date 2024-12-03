using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Advertisements.Events;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements.Events;

public class AdvertisementClosedDomainEventHandler(
    IAdvertisementRepository advertisementRepository,
    IPersonRepository personRepository)
    : INotificationHandler<AdvertisementClosedDomainEvent>
{
    public async Task Handle(AdvertisementClosedDomainEvent notification, CancellationToken cancellationToken)
    {
        Advertisement advertisement = await advertisementRepository.GetAdvertisementByIdAsync(notification.AdvertisementId, cancellationToken);

        Person person = await personRepository.GetPersonByIdAsync(advertisement.PersonId, cancellationToken);

        person.MarkCatsFromConcreteAdvertisementAsAdopted(advertisement.Id);
    }
}