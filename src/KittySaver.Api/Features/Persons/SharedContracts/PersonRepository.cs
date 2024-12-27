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

    public void Remove(Person person) => writeDb.Persons.Remove(person);
}