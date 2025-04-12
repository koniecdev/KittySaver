using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KittySaver.Shared.Requests;
using KittySaver.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Infrastructure.Clients;


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
    public Task<Guid> RegisterAsync(RegisterRequest request);
    public Task DeletePersonAsync(Guid id);
}

public class AuthApiHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : IAuthApiHttpClient
{
    private static class Exceptions
    {
        public sealed class DeserializationOfApiResponseException() 
            : Exception("Something went wrong with deserialization of external api response");
    }

    public async Task<Guid> RegisterAsync(RegisterRequest request)
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
        IdResponse idResponse = await response.Content.ReadFromJsonAsync<IdResponse>()
            ?? throw new Exceptions.DeserializationOfApiResponseException();
        return idResponse.Id;
    }
    
    public async Task DeletePersonAsync(Guid id)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);
        string authHeader = httpContextAccessor.HttpContext.Request.Headers.Authorization.ToString();
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