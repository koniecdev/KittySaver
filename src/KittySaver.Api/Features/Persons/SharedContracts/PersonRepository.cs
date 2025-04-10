﻿using KittySaver.Api.Infrastructure.Clients;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public class PersonRepository(ApplicationWriteDbContext writeDb, IAuthApiHttpClient authApiHttpClient) : IPersonRepository
{
    public async Task<Person> GetPersonByIdAsync(PersonId id, CancellationToken cancellationToken)
        => await writeDb.Persons
            .Where(person => person.Id == id)
            .Include(person => person.Cats)
            .Include(person => person.Advertisements)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new NotFoundExceptions.PersonNotFoundException(id);
    
    public async Task InsertAsync(Person person, string password)
    {
        writeDb.Persons.Add(person);
        
        RegisterRequest registerDto = new(
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