using System.Net.Http.Json;
using System.Text.Json;
using Polly;
using Polly.Extensions.Http;

namespace KittySaver.Wasm.Shared.HttpClients;

public interface IApiClient
{
    Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
}

public class ApiClient(
    HttpClient httpClient,
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
            logger.LogInformation("Making GET request to {Endpoint}", endpoint);
            
            var response = await httpClient.GetAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Making POST request to {Endpoint}", endpoint);
            
            var response = await httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Making PUT request to {Endpoint}", endpoint);
            
            var response = await httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Making DELETE request to {Endpoint}", endpoint);
            
            var response = await httpClient.DeleteAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error making DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }

    private async Task EnsureSuccessStatusCodeWithLoggingAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
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
    public static IServiceCollection AddApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? "https://api.your-backend.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}