using KittySaver.Api.Shared.Infrastructure.Clients;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public class PersonRepository(ApplicationWriteDbContext writeDb, IAuthApiHttpClient authApiHttpClient) : IPersonRepository
{
    public async Task<Person> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken)
        => await writeDb.Persons
            .Where(person => person.Id == id)
            .Include(person => person.Cats)
            .Include(person => person.Advertisements)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new NotFoundExceptions.PersonNotFoundException(id);
    
    public async Task InsertAsync(Person person, string password)
    {
        writeDb.Persons.Add(person);
        
        IAuthApiHttpClient.RegisterDto registerDto = new(
            UserName: person.Nickname.Value,
            Email: person.Email.Value,
            PhoneNumber: person.PhoneNumber.Value,
            Password: password);
        
        Guid userIdentityId = await authApiHttpClient.RegisterAsync(registerDto);
        person.SetUserIdentityId(userIdentityId);
    }

    public async Task RemoveAsync(Person person, string authHeader)
    {
        writeDb.Persons.Remove(person);
        await authApiHttpClient.DeletePersonAsync(person.UserIdentityId, authHeader);
    }
}