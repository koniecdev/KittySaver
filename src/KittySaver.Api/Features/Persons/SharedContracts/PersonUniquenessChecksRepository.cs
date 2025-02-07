using KittySaver.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public interface IPersonUniquenessChecksRepository
{
    public Task<bool> IsPhoneNumberUniqueAsync(string phone, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken);
    public Task<bool> IsEmailUniqueAsync(string email, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken);
}

public class PersonUniquenessChecksRepository(ApplicationWriteDbContext writeDb) : IPersonUniquenessChecksRepository
{
    public async Task<bool> IsPhoneNumberUniqueAsync(string phone, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken) 
        => userToExcludeIdOrIdentityId is null 
            ? !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(person => person.PhoneNumber.Value == phone, cancellationToken)
            : !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(x => 
                        x.PhoneNumber.Value == phone 
                        && x.Id != userToExcludeIdOrIdentityId 
                        && x.UserIdentityId != userToExcludeIdOrIdentityId,
                    cancellationToken);
        
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? userToExcludeIdOrIdentityId, CancellationToken cancellationToken) 
        => userToExcludeIdOrIdentityId is null 
            ? !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(person => person.Email.Value == email, cancellationToken)
            : !await writeDb.Persons
                .AsNoTracking()
                .AnyAsync(x =>
                        x.Email.Value == email 
                        && x.Id != userToExcludeIdOrIdentityId 
                        && x.UserIdentityId != userToExcludeIdOrIdentityId,
                    cancellationToken);
}