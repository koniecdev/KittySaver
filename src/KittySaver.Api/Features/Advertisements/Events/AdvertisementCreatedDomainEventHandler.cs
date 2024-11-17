using KittySaver.Api.Shared.Domain.Advertisement;
using KittySaver.Api.Shared.Domain.Advertisement.Events;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements.Events;

public class AdvertisementCreatedDomainEventHandler(ApplicationDbContext db)
    : INotificationHandler<AdvertisementCreatedDomainEvent>
{
    public async Task Handle(AdvertisementCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Person person =
            await db.Persons
                .Where(x => x.Id == notification.PersonId)
                .Include(x => x.Cats)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundExceptions.PersonNotFoundException(notification.PersonId);

        foreach (Guid catId in notification.IdsOfCatsToAssignToAdvertisement)
        {
            person.AssignCatToAdvertisement(notification.AdvertisementId, catId);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}