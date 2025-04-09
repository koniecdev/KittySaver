using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public interface IPersonUniquenessChecksRepository
{
    public Task<bool> IsPhoneNumberUniqueAsync(string phone, PersonId? userToExcludeId, CancellationToken cancellationToken);
    public Task<bool> IsEmailUniqueAsync(string email, PersonId? userToExcludeId, CancellationToken cancellationToken);
}

public class PersonUniquenessChecksRepository(ApplicationWriteDbContext writeDb) : IPersonUniquenessChecksRepository
{
    public async Task<bool> IsPhoneNumberUniqueAsync(string phone, PersonId? userToExcludeId, CancellationToken cancellationToken) 
        => userToExcludeId is null 
            ? !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(person => person.PhoneNumber.Value == phone, cancellationToken)
            : !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(x => 
                        x.PhoneNumber.Value == phone 
                        && x.Id != userToExcludeId,
                    cancellationToken);
        
    public async Task<bool> IsEmailUniqueAsync(string email, PersonId? userToExcludeId, CancellationToken cancellationToken) 
        => userToExcludeId is null 
            ? !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(person => person.Email.Value == email, cancellationToken)
            : !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(x =>
                        x.Email.Value == email 
                        && x.Id != userToExcludeId ,
                    cancellationToken);
}