using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats.Events;

public class AssignedToAdvertisementCatStatusChangedDomainEventHandler(
    IAdvertisementRepository advertisementRepository,
    IPersonRepository personRepository)
    : INotificationHandler<AssignedToAdvertisementCatStatusChangedDomainEvent>
{
    public async Task Handle(AssignedToAdvertisementCatStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        AdvertisementService advertisementService = new(advertisementRepository, personRepository);
        await advertisementService.RecalculatePriorityScoreAsync(notification.AdvertisementId, cancellationToken);
    }
}