using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Domain.Persons.DomainRepositories;

public interface IPersonRepository
{
    public Task<Person> GetPersonByIdAsync(PersonId id, CancellationToken cancellationToken);
    public Task InsertAsync(Person person, string password);
    public Task RemoveAsync(Person person, string authHeader);
}