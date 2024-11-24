using KittySaver.Api.Shared.Domain.Advertisements;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.Persons.Events;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats.Events;

public class AssignedToAdvertisementCatStatusChangedDomainEventHandler(ApplicationDbContext db)
    : INotificationHandler<AssignedToAdvertisementCatStatusChangedDomainEvent>
{
    public async Task Handle(AssignedToAdvertisementCatStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Advertisement advertisement =
            await db.Advertisements
                .FirstOrDefaultAsync(x => x.Id == notification.AdvertisementId, cancellationToken)
            ?? throw new NotFoundExceptions.AdvertisementNotFoundException(notification.AdvertisementId);

        Person person = await db.Persons
            .Where(x => x.Id == advertisement.PersonId)
            .Include(x => x.Cats)
            .FirstAsync(cancellationToken);
        
        AdvertisementService advertisementService = new();
        advertisementService.RecalculatePriorityScore(person, advertisement);
    }
}