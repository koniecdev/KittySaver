namespace KittySaver.Domain.Persons;

public interface IPersonRepository
{
    public Task<Person> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<Person> GetPersonByIdOrIdentityIdAsync(Guid idOrIdentityId, CancellationToken cancellationToken);
    public void Insert(Person person);
    public void Remove(Person person);
}