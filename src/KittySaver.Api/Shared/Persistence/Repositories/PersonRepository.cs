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
    
    public async Task<bool> IsPhoneNumberUniqueAsync(string phone, CancellationToken cancellationToken) 
        => !await db.Persons
            .AsNoTracking()
            .AnyAsync(person => person.PhoneNumber.Value == phone, cancellationToken);
        
    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken) 
        => !await db.Persons
            .AsNoTracking()
            .AnyAsync(person => person.Email.Value == email, cancellationToken);

    public async Task<bool> IsUserIdentityIdUniqueAsync(Guid userIdentityId, CancellationToken cancellationToken) 
        => !await db.Persons
            .AsNoTracking()
            .AnyAsync(person => person.UserIdentityId == userIdentityId, cancellationToken);

    public void Insert(Person person)
    {
        db.Persons.Add(person);
    }
}