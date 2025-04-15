using KittySaver.Api.Infrastructure.Clients;
using KittySaver.Api.Persistence;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public class PersonRepository(ApplicationWriteDbContext db) : GenericRepository<Person, PersonId>(db), IPersonRepository
{
    public override async Task<Person> GetByIdAsync(PersonId id, CancellationToken cancellationToken)
        => await DbContext.Persons
            .Where(person => person.Id == id)
            .Include(person => person.Cats)
            .Include(person => person.Advertisements)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new NotFoundException<PersonId>(nameof(Person), id);
}