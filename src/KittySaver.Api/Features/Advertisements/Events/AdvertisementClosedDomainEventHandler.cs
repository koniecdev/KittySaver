using KittySaver.Api.Shared.Domain.Advertisement;
using KittySaver.Api.Shared.Domain.Advertisement.Events;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements.Events;

public class AdvertisementClosedDomainEventHandler(ApplicationDbContext db)
    : INotificationHandler<AdvertisementClosedDomainEvent>
{
    public async Task Handle(AdvertisementClosedDomainEvent notification, CancellationToken cancellationToken)
    {
        Advertisement advertisement =
            await db.Advertisements
                .Where(x => x.Id == notification.AdvertisementId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundExceptions.AdvertisementNotFoundException(notification.AdvertisementId);

        Person person =
            await db.Persons
                .Where(x => x.Id == advertisement.PersonId)
                .Include(x => x.Cats)
                .FirstAsync(cancellationToken);

        List<Cat> catsThatAreAssignedToClosedAdvertisement =
            person.Cats
                .Where(x => x.AdvertisementId == advertisement.Id)
                .ToList();

        foreach (Cat cat in catsThatAreAssignedToClosedAdvertisement)
        {
            cat.MarkAsAdopted();
        }
    }
}