using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace KittySaver.Wasm.Shared.HttpClients;

public interface IApiClient
{
    Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TResponse>(string endpointUrl, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PutFileAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}

public class ApiClient(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    ILogger<ApiClient> logger) : IApiClient
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            logger.LogInformation("Making GET request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await httpClient.GetAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TResponse>(string endpointUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage response = await httpClient.PostAsync(endpointUrl, null, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            //Well, we do need the Result pattern for Response<TResponse || ProblemDetails>
            logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }
    
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(endpointUrl, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            //Well, we do need the Result pattern for Response<TResponse || ProblemDetails>
            logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            logger.LogInformation("Making PUT request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            logger.LogInformation("Making DELETE request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await httpClient.DeleteAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }
    
    public async Task<TResponse?> PutFileAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            logger.LogInformation("Making PUT FILE request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await httpClient.PutAsync(endpoint, content, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            TResponse? toReturn = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return toReturn;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making PUT FILE request to {Endpoint}", endpoint);
            throw;
        }
    }
    
    private async Task SetAuthorizationHeadersIfPresent()
    {
        string? token = await localStorageService.GetItemAsStringAsync("token");
        string? tokenExpiresAsString = await localStorageService.GetItemAsStringAsync("token_expires");
        if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(tokenExpiresAsString))
        {
            DateTimeOffset tokenExpiresAt = DateTimeOffset.Parse(tokenExpiresAsString.Replace("\"", ""));
            if (DateTimeOffset.Now > tokenExpiresAt)
            {
                //Get refresh token => issue new jwt => replace jwt
                //or if refresh token is expired
                //remove refresh token from storage && remove jwt from storage => redirect to login page
                await localStorageService.ClearAsync();
            }
            token = token.Replace("\"", "");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return;
        }
        await localStorageService.ClearAsync();
        httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task EnsureSuccessStatusCodeWithLoggingAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            logger.LogError("HTTP {StatusCode} response from {RequestUri}: {Content}",
                (int)response.StatusCode,
                response.RequestMessage?.RequestUri,
                content);
        }
        
        response.EnsureSuccessStatusCode();
    }
}

public static class HttpClientExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services)
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}