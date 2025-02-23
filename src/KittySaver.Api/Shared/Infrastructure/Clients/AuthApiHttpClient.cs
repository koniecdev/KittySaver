using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KittySaver.Shared.Requests;
using KittySaver.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Shared.Infrastructure.Clients;


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


public interface IAuthApiHttpClient
{
    protected record IdResponse(Guid Id);
    public sealed record RegisterDto(
        string UserName,
        string Email,
        string PhoneNumber,
        string Password);
    public Task<Guid> RegisterAsync(RegisterDto request);
    public Task DeletePersonAsync(Guid id, string authToken);
}

public class AuthApiHttpClient(HttpClient client) : IAuthApiHttpClient
{
    private static class Exceptions
    {
        public sealed class DeserializationOfApiResponseException() 
            : Exception("Something went wrong with deserialization of external api response");
    }

    public async Task<Guid> RegisterAsync(IAuthApiHttpClient.RegisterDto request)
    {
        string serializedRequest = JsonSerializer.Serialize(request);
        StringContent bodyContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("v1/application-users", bodyContent);
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
        IAuthApiHttpClient.IdResponse idResponse = await response.Content.ReadFromJsonAsync<IAuthApiHttpClient.IdResponse>()
            ?? throw new Exceptions.DeserializationOfApiResponseException();
        return idResponse.Id;
    }
    
    public async Task DeletePersonAsync(Guid id, string authHeader)
    {
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
        HttpResponseMessage response = await client.DeleteAsync($"v1/application-users/{id}");
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