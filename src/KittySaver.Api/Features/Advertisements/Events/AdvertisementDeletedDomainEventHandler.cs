using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements.Events;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements.Events;

public class AdvertisementDeletedDomainEventHandler(ApplicationDbContext db)
    : INotificationHandler<AdvertisementDeletedDomainEvent>
{
    public async Task Handle(AdvertisementDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Person person =
            await db.Persons
                .Where(x => x.Id == notification.OwnerPersonId)
                .Include(x => x.Cats)
                .FirstAsync(cancellationToken);

        person.UnassignCatsFromRemovedAdvertisement(notification.AdvertisementId);
    }
}