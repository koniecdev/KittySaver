namespace KittySaver.Domain.Persons;

public interface IPersonRepository
{
    public Task<Person> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<Person> GetPersonByIdOrIdentityIdAsync(Guid idOrIdentityId, CancellationToken cancellationToken);
    public Task InsertAsync(Person person, string password);
    public Task RemoveAsync(Person person, string authHeader);
}