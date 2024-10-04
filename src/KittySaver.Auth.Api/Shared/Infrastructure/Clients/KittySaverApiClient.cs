using System.Net;
using System.Text;
using System.Text.Json;
using KittySaver.Auth.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

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
    public sealed record PersonDto(Guid Id, string Email, string FullName, string PhoneNumber);
    public sealed record CreatePersonDto(string FirstName, string LastName, string Email, string PhoneNumber, Guid UserIdentityId);
    public sealed record UpdatePersonDto(string FirstName, string LastName, string Email, string PhoneNumber);
    Task<ICollection<PersonDto>> GetPersons();
    Task<PersonDto> GetPerson(Guid id);
    public Task<Guid> CreatePerson(CreatePersonDto request);
    public Task UpdatePerson(Guid id, UpdatePersonDto request);
    public Task DeletePerson(Guid id);
}

public class KittySaverApiClient(HttpClient client) : IKittySaverApiClient
{
    public static class Exceptions
    {
        public sealed class DeserializationOfApiResponseException() 
            : BadRequestException("ExternalApi.Response", "Something went wrong with deserialization of external api response");
    }
    public async Task<ICollection<IKittySaverApiClient.PersonDto>> GetPersons()
    {
        HttpResponseMessage response = await client.GetAsync("v1/persons");
        ICollection<IKittySaverApiClient.PersonDto>? dtos = await response.Content.ReadFromJsonAsync<ICollection<IKittySaverApiClient.PersonDto>>();
        dtos ??= new List<IKittySaverApiClient.PersonDto>();
        return dtos;
    }
    
    public async Task<IKittySaverApiClient.PersonDto> GetPerson(Guid id)
    {
        HttpResponseMessage response = await client.GetAsync($"v1/persons/{id}");
        IKittySaverApiClient.PersonDto dto = await response.Content.ReadFromJsonAsync<IKittySaverApiClient.PersonDto>()
            ?? throw new Exceptions.DeserializationOfApiResponseException();
        return dto;
    }

    public async Task<Guid> CreatePerson(IKittySaverApiClient.CreatePersonDto request)
    {
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