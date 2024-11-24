using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Advertisements.Events;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
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

        person.MarkCatsFromConcreteAdvertisementAsAdopted(advertisement.Id);
    }
}