using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Repositories;

public class PersonRepository(ApplicationDbContext db) : IPersonRepository
{
    public async Task<IEnumerable<Person>> GetAllPersonsAsync(CancellationToken cancellationToken)
        => await db.Persons
            .Include(person => person.Cats)
            .ToListAsync(cancellationToken);
    
    public async Task<Person> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken)
        => await db.Persons
            .Where(person => person.Id == id)
            .Include(person => person.Cats)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new NotFoundExceptions.PersonNotFoundException(id);
    
    public async Task<Person> GetPersonByIdOrIdentityIdAsync(Guid idOrUserIdentityId, CancellationToken cancellationToken)
        => await db.Persons
               .Where(person => person.Id == idOrUserIdentityId || person.UserIdentityId == idOrUserIdentityId)
               .Include(person => person.Cats)
               .FirstOrDefaultAsync(cancellationToken)
               ?? throw new NotFoundExceptions.PersonNotFoundException(idOrUserIdentityId);
    public void Insert(Person person)
    {
        db.Persons.Add(person);
    }

    public void Remove(Person person)
    {
        db.Persons.Remove(person);
        person.AnnounceDeletion();
    }
    
    public async Task<bool> IsPhoneNumberUniqueAsync(string phone, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken) 
        => userToExcludeIdOrIdentityId is null 
            ? !await db.Persons
                .AsNoTracking()
                .AnyAsync(person => person.PhoneNumber.Value == phone, cancellationToken)
            : !await db.Persons
                .AsNoTracking()
                .AnyAsync(x => 
                    x.PhoneNumber.Value == phone 
                    && x.Id != userToExcludeIdOrIdentityId 
                    && x.UserIdentityId != userToExcludeIdOrIdentityId,
                    cancellationToken);
        
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken) 
        => userToExcludeIdOrIdentityId is null 
            ? !await db.Persons
                .AsNoTracking()
                .AnyAsync(person => person.Email.Value == email, cancellationToken)
            : !await db.Persons
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Email.Value == email 
                    && x.Id != userToExcludeIdOrIdentityId 
                    && x.UserIdentityId != userToExcludeIdOrIdentityId,
                    cancellationToken);

    public async Task<bool> IsUserIdentityIdUniqueAsync(Guid userIdentityId, CancellationToken cancellationToken) 
        => !await db.Persons
            .AsNoTracking()
            .AnyAsync(person => person.UserIdentityId == userIdentityId, cancellationToken);
}