namespace KittySaver.Domain.Persons;

public interface IPersonRepository
{
    public Task<IEnumerable<Person>> GetAllPersonsAsync(CancellationToken cancellationToken);
    public Task<Person> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<Person> GetPersonByIdOrIdentityIdAsync(Guid idOrIdentityId, CancellationToken cancellationToken);
    public Task<bool> IsPhoneNumberUniqueAsync(string phone, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken);
    public Task<bool> IsEmailUniqueAsync(string email, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken);
    public Task<bool> IsUserIdentityIdUniqueAsync(Guid userIdentityId, CancellationToken cancellationToken);
    public void Insert(Person person);
    public void Remove(Person person);
    public Task RemoveAllPersonAdvertisementsAsync(Guid personId, CancellationToken cancellationToken);
}