using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public class PersonRepository(ApplicationWriteDbContext writeDb) : IPersonRepository
{
    public async Task<Person> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken)
        => await writeDb.Persons
            .Where(person => person.Id == id)
            .Include(person => person.Cats)
            .Include(person => person.Advertisements)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new NotFoundExceptions.PersonNotFoundException(id);
    
    public async Task<Person> GetPersonByIdOrIdentityIdAsync(Guid idOrUserIdentityId, CancellationToken cancellationToken)
        => await writeDb.Persons
               .Where(person => person.Id == idOrUserIdentityId || person.UserIdentityId == idOrUserIdentityId)
               .Include(person => person.Cats)
               .Include(person => person.Advertisements)
               .FirstOrDefaultAsync(cancellationToken)
               ?? throw new NotFoundExceptions.PersonNotFoundException(idOrUserIdentityId);
    
    public void Insert(Person person) => writeDb.Persons.Add(person);

    public void Remove(Person person)
    {
        writeDb.Persons.Remove(person);
    }

    public async Task<bool> IsPhoneNumberUniqueAsync(string phone, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken) 
        => userToExcludeIdOrIdentityId is null 
            ? !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(person => person.PhoneNumber.Value == phone, cancellationToken)
            : !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(x => 
                    x.PhoneNumber.Value == phone 
                    && x.Id != userToExcludeIdOrIdentityId 
                    && x.UserIdentityId != userToExcludeIdOrIdentityId,
                    cancellationToken);
        
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken) 
        => userToExcludeIdOrIdentityId is null 
            ? !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(person => person.Email.Value == email, cancellationToken)
            : !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Email.Value == email 
                    && x.Id != userToExcludeIdOrIdentityId 
                    && x.UserIdentityId != userToExcludeIdOrIdentityId,
                    cancellationToken);

    public async Task<bool> IsUserIdentityIdUniqueAsync(Guid userIdentityId, CancellationToken cancellationToken) 
        => !await writeDb.Persons
            .AsNoTracking()
            .AnyAsync(person => person.UserIdentityId == userIdentityId, cancellationToken);
}