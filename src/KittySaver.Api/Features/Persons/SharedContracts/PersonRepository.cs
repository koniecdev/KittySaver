using KittySaver.Api.Infrastructure.Clients;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public class PersonRepository(ApplicationWriteDbContext writeDb) : IPersonRepository
{
    public async Task<Person> GetPersonByIdAsync(PersonId id, CancellationToken cancellationToken)
        => await writeDb.Persons
            .Where(person => person.Id == id)
            .Include(person => person.Cats)
            .Include(person => person.Advertisements)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new NotFoundExceptions.PersonNotFoundException(id);
    
    public void Insert(Person person)
    {
        writeDb.Persons.Add(person);
    }

    public void Remove(Person person)
    {
        writeDb.Persons.Remove(person);
    }
}