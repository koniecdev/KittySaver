using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KittySaver.Shared.Requests;
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
    public Task<TResponse?> RegisterAsync<TResponse>(RegisterRequest request,
        CancellationToken cancellationToken);

    public Task DeletePersonAsync(Guid id);
}

public class AuthApiHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : IAuthApiHttpClient
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static class Exceptions
    {
        public sealed class DeserializationOfApiResponseException()
            : Exception("Something went wrong with deserialization of external api response");
    }

    public async Task<TResponse?> RegisterAsync<TResponse>(RegisterRequest request,
        CancellationToken cancellationToken)
    {
        string serializedRequest = JsonSerializer.Serialize(request);
        StringContent bodyContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        HttpResponseMessage responseMessage =
            await client.PostAsync("v1/application-users", bodyContent, cancellationToken);

        if (responseMessage.IsSuccessStatusCode)
        {
            return await responseMessage.Content.ReadFromJsonAsync<TResponse?>(_jsonOptions, cancellationToken);
        }

        // Zachowujemy oryginalną zawartość, aby nie odczytywać jej wielokrotnie
        var contentString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        // Najpierw próbujemy ValidationProblemDetails
        if (responseMessage.StatusCode is HttpStatusCode.BadRequest)
        {
            try
            {
                var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(
                    contentString, _jsonOptions);
                
                if (validationProblemDetails is not null && validationProblemDetails.Errors.Count > 0)
                {
                    throw new ApiValidationException(validationProblemDetails);
                }
            }
            catch (JsonException)
            {
                // Ignorujemy wyjątek deserializacji i próbujemy następnej opcji
            }
        }

        // Następnie próbujemy zwykły ProblemDetails
        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(contentString, _jsonOptions);
            
            if (problemDetails is not null)
            {
                throw new ApiException(problemDetails);
            }
        }
        catch (JsonException)
        {
            // Ignorujemy wyjątek deserializacji
        }

        // Jeśli wszystko zawiedzie, rzucamy ogólny wyjątek
        throw new Exceptions.DeserializationOfApiResponseException();
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
            // Zachowujemy oryginalną zawartość
            var contentString = await response.Content.ReadAsStringAsync();
            
            // Najpierw próbujemy ValidationProblemDetails
            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                try
                {
                    var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(
                        contentString, _jsonOptions);
                    
                    if (validationProblemDetails is not null)
                    {
                        throw new ApiValidationException(validationProblemDetails);
                    }
                }
                catch (JsonException)
                {
                    // Ignorujemy wyjątek deserializacji i próbujemy następnej opcji
                }
            }

            // Następnie próbujemy zwykły ProblemDetails
            try
            {
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(contentString, _jsonOptions);
                
                if (problemDetails is not null)
                {
                    throw new ApiException(problemDetails);
                }
            }
            catch (JsonException)
            {
                // Ignorujemy wyjątek deserializacji
            }

            // Jeśli wszystko zawiedzie, rzucamy ogólny wyjątek
            throw new Exceptions.DeserializationOfApiResponseException();
        }
    }
}