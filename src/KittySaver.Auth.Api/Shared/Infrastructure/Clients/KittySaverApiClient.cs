using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Persistence;
using KittySaver.Auth.Api.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Shared.Infrastructure.Clients;

public class ApiException(ProblemDetails problemDetails)
    : Exception($"Api responded with status code: {problemDetails.Status} and message: {problemDetails.Detail}")
{
    public ProblemDetails ProblemDetails { get; } = problemDetails;
}
public class ApiValidationException(ValidationProblemDetails problemDetails)
    : Exception($"Api responded with status code: {problemDetails.Status} and message: {problemDetails.Detail}")
{
    public ValidationProblemDetails ValidationProblemDetails { get; } = problemDetails;
}

public interface IKittySaverApiClient
{
    protected record IdResponse(Guid Id);
    public sealed record PersonDto(Guid Id, string Email, string Username, string PhoneNumber);
    public sealed record CreatePersonDto(
        string Nickname,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber);
    public sealed record UpdatePersonDto(
        string Nickname,
        string Email,
        string PhoneNumber,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber);
    Task<ICollection<PersonDto>> GetPersons();
    Task<PersonDto> GetPerson(Guid id);
    public Task<Guid> CreatePerson(string jwt, CreatePersonDto request);
    public Task UpdatePerson(Guid id, UpdatePersonDto request);
    public Task DeletePerson(Guid id);
}

public class KittySaverApiClient(
    HttpClient client,
    IJwtTokenService jwtTokenService,
    ICurrentUserService currentUserService,
    ApplicationDbContext db) : IKittySaverApiClient
{
    private static class Exceptions
    {
        public sealed class DeserializationOfApiResponseException() 
            : BadRequestException("ExternalApi.Response", "Something went wrong with deserialization of external api response");
    }

    private async Task SetUpAuthorization()
    {
        string userId = currentUserService.UserId;
        ApplicationUser? user = await db.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        if (user is not null)
        {
            var (token, _) = await jwtTokenService.GenerateTokenAsync(user);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    public async Task<ICollection<IKittySaverApiClient.PersonDto>> GetPersons()
    {
        await SetUpAuthorization();
        HttpResponseMessage response = await client.GetAsync("v1/persons");
        ICollection<IKittySaverApiClient.PersonDto>? dtos = await response.Content.ReadFromJsonAsync<ICollection<IKittySaverApiClient.PersonDto>>();
        dtos ??= new List<IKittySaverApiClient.PersonDto>();
        return dtos;
    }
    
    public async Task<IKittySaverApiClient.PersonDto> GetPerson(Guid id)
    {
        await SetUpAuthorization();
        HttpResponseMessage response = await client.GetAsync($"v1/persons/{id}");
        IKittySaverApiClient.PersonDto dto = await response.Content.ReadFromJsonAsync<IKittySaverApiClient.PersonDto>()
            ?? throw new Exceptions.DeserializationOfApiResponseException();
        return dto;
    }

    public async Task<Guid> CreatePerson(string jwt, IKittySaverApiClient.CreatePersonDto request)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        string serializedRequest = JsonSerializer.Serialize(request);
        StringContent bodyContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("v1/persons", bodyContent);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                ValidationProblemDetails? validationProblemDetails =
                    await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationProblemDetails is not null)
                {
                    throw new ApiValidationException(validationProblemDetails);
                }
            }
            ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails is not null)
            {
                throw new ApiException(problemDetails);
            }

            throw new Exceptions.DeserializationOfApiResponseException();
        }
        IKittySaverApiClient.IdResponse idResponse = await response.Content.ReadFromJsonAsync<IKittySaverApiClient.IdResponse>()
            ?? throw new Exceptions.DeserializationOfApiResponseException();
        return idResponse.Id;
    }
    
    public async Task UpdatePerson(Guid id, IKittySaverApiClient.UpdatePersonDto request)
    {
        await SetUpAuthorization();
        string serializedRequest = JsonSerializer.Serialize(request);
        StringContent bodyContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync($"v1/persons/{id}", bodyContent);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationProblemDetails is not null)
                {
                    throw new ApiValidationException(validationProblemDetails); 
                }
            }
            ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails is not null)
            {
                throw new ApiException(problemDetails);
            }
            throw new Exceptions.DeserializationOfApiResponseException();
        }
    }
    
    public async Task DeletePerson(Guid id)
    {
        await SetUpAuthorization();
        HttpResponseMessage response = await client.DeleteAsync($"v1/persons/{id}");
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                ValidationProblemDetails? validationProblemDetails =
                    await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationProblemDetails is not null)
                {
                    throw new ApiValidationException(validationProblemDetails); 
                }
            }
            ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails is not null)
            {
                throw new ApiException(problemDetails);
            }
            throw new Exceptions.DeserializationOfApiResponseException();
        }
    }
}