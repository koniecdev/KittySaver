using KittySaver.Api.Shared.Domain.Persons.Events;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.Events;

public class PersonDeletedDomainEventHandler(ApplicationDbContext db) : INotificationHandler<PersonDeletedDomainEvent>
{
    public async Task Handle(PersonDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        await db.Advertisements
            .Where(x => x.PersonId == notification.PersonId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}