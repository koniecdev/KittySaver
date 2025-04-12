using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using KittySaver.Wasm.Shared.Converters;
using KittySaver.Wasm.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace KittySaver.Wasm.Shared.HttpClients;

public interface IApiClient
{
    Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
    Task PostAsync(string endpointUrl, CancellationToken cancellationToken = default);
    Task<Result<TResponse?>> PostWithResultAsync<TResponse>(string endpointUrl, CancellationToken cancellationToken = default);
    Task<Result<TResponse?>> PostWithResultAsync<TRequest, TResponse>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TResponse>(string endpointUrl, CancellationToken cancellationToken = default);
    Task PostAsync<TRequest>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PostFileAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task<TResponse?> PutFileAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}

public class ApiClient : IApiClient
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorageService;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient,
        ILocalStorageService localStorageService,
        ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _localStorageService = localStorageService;
        _logger = logger;
        _jsonOptions.Converters.Add(new ValidationProblemDetailsConverter());
    }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making GET request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task PostAsync(string endpointUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpointUrl, null, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            //Well, we do need the Result pattern for Response<TResponse || ProblemDetails>
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }
    
    public async Task<TResponse?> PostAsync<TResponse>(string endpointUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpointUrl, null, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            //Well, we do need the Result pattern for Response<TResponse || ProblemDetails>
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }
    
    public async Task<Result<TResponse?>> PostWithResultAsync<TResponse>(string endpointUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage responseMessage = await _httpClient.PostAsync(endpointUrl, null, cancellationToken);
            Result<TResponse?> result = await ProcessHttpMessageResponse<TResponse>(responseMessage, cancellationToken);
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }

    public async Task<Result<TResponse?>> PostWithResultAsync<TRequest, TResponse>(
        string endpointUrl,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpointUrl, request, _jsonOptions, cancellationToken);
            Result<TResponse?> result = await ProcessHttpMessageResponse<TResponse>(responseMessage, cancellationToken);
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }
    
    public async Task PostAsync<TRequest>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpointUrl, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            //Well, we do need the Result pattern for Response<TResponse || ProblemDetails>
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpointUrl, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST request to {Endpoint}", endpointUrl);
            
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpointUrl, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            //Well, we do need the Result pattern for Response<TResponse || ProblemDetails>
            
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpointUrl);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making PUT request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making DELETE request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }
    
    public async Task<TResponse?> PostFileAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making POST FILE request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            TResponse? toReturn = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return toReturn;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making PUT FILE request to {Endpoint}", endpoint);
            throw;
        }
    }
    
    public async Task<TResponse?> PutFileAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAuthorizationHeadersIfPresent();
            _logger.LogInformation("Making PUT FILE request to {Endpoint}", endpoint);
            
            HttpResponseMessage response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
            await EnsureSuccessStatusCodeWithLoggingAsync(response);
            TResponse? toReturn = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return toReturn;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error making PUT FILE request to {Endpoint}", endpoint);
            throw;
        }
    }
    
    private async Task<Result<TResponse?>> ProcessHttpMessageResponse<TResponse>(
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            Result<TResponse?> response = await responseMessage.Content.ReadFromJsonAsync<TResponse?>(_jsonOptions, cancellationToken);
            return response;
        }

        switch (responseMessage.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                try
                {
                    // var jsonString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                    ValidationProblemDetails validationProblemDetails = 
                        await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>(_jsonOptions, cancellationToken)
                        ?? throw new JsonException();
                    return Result.Invalid(validationProblemDetails.ToValidationErrors());
                }
                catch
                {
                    try
                    {
                        ProblemDetails validationProblemDetails = 
                            await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions, cancellationToken)
                            ?? throw new JsonException();
                        return Result.Invalid(validationProblemDetails.ToValidationErrors());
                    }
                    catch (Exception e)
                    {
                        return Result.Invalid(new ValidationError(e.Message));
                    }
                }
            default:
                try
                {
                    ProblemDetails validationProblemDetails = 
                        await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions, cancellationToken)
                        ?? throw new JsonException();
                    return Result.Invalid(validationProblemDetails.ToValidationErrors());
                }
                catch (Exception e)
                {
                    return Result.Invalid(new ValidationError(e.Message));
                }
        }
    }
    
    private async Task SetAuthorizationHeadersIfPresent()
    {
        string? token = await _localStorageService.GetItemAsStringAsync("token");
        string? tokenExpiresAsString = await _localStorageService.GetItemAsStringAsync("token_expires");
        if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(tokenExpiresAsString))
        {
            DateTimeOffset tokenExpiresAt = DateTimeOffset.Parse(tokenExpiresAsString.Replace("\"", ""));
            if (DateTimeOffset.Now > tokenExpiresAt)
            {
                //Get refresh token => issue new jwt => replace jwt
                //or if refresh token is expired
                //remove refresh token from storage && remove jwt from storage => redirect to login page
                await _localStorageService.ClearAsync();
            }
            token = token.Replace("\"", "");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return;
        }
        await _localStorageService.ClearAsync();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task EnsureSuccessStatusCodeWithLoggingAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            _logger.LogError("HTTP {StatusCode} response from {RequestUri}: {Content}",
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