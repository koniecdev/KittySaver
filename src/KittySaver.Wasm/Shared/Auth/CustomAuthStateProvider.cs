using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KittySaver.Shared.Requests;
using KittySaver.Shared.Responses;
using KittySaver.Wasm.Shared.HttpClients;
using Microsoft.AspNetCore.Components.Authorization;

namespace KittySaver.Wasm.Shared.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly IApiClient _apiClient;
    private Timer? _tokenRefreshTimer;

    public CustomAuthStateProvider(ILocalStorageService localStorageService, IApiClient apiClient)
    {
        _localStorageService = localStorageService;
        _apiClient = apiClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await _localStorageService.GetItemAsStringAsync("token");
        ClaimsIdentity identity = new();
    
        if (!string.IsNullOrWhiteSpace(token))
        {
            token = token.Replace("\"", "");
        
            bool isTokenExpired = IsTokenExpired(token);
        
            if (!isTokenExpired)
            {
                identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                
                // Setup refresh timer if needed
                SetupRefreshTimer(token);
            }
            else
            {
                // Try to refresh the token
                bool refreshed = await TryRefreshToken();
                if (refreshed)
                {
                    // Get the new token and create identity
                    string? newToken = await _localStorageService.GetItemAsStringAsync("token");
                    if (!string.IsNullOrWhiteSpace(newToken))
                    {
                        newToken = newToken.Replace("\"", "");
                        identity = new ClaimsIdentity(ParseClaimsFromJwt(newToken), "jwt");
                    }
                }
                else
                {
                    // Clear tokens if refresh failed
                    await ClearAuthData();
                }
            }
        }
    
        ClaimsPrincipal user = new(identity);
        AuthenticationState state = new(user);
    
        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return state;
    }

    private static bool IsTokenExpired(string token)
    {
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
    
        DateTime expiry = jwtToken.ValidTo;
    
        return expiry < DateTime.UtcNow;
    }
    
    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
    
    private async Task<bool> TryRefreshToken()
    {
        try
        {
            string? refreshToken = await _localStorageService.GetItemAsStringAsync("refresh_token");
            string? accessToken = await _localStorageService.GetItemAsStringAsync("token");
            
            if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(accessToken))
            {
                return false;
            }
            
            refreshToken = refreshToken.Replace("\"", "");
            accessToken = accessToken.Replace("\"", "");
            
            // Call the refresh token endpoint
            LoginResponse? response = await _apiClient.PostAsync<RefreshTokenRequest, LoginResponse>(
                $"{StaticDetails.AuthUrl}application-users/refresh-token",
                new RefreshTokenRequest(accessToken, refreshToken));

            if (response is null)
            {
                return false;
            }
            // Store the new tokens
            await _localStorageService.SetItemAsync("token", response.AccessToken);
            await _localStorageService.SetItemAsync("token_expires", response.AccessTokenExpiresAt);
            await _localStorageService.SetItemAsync("refresh_token", response.RefreshToken);
            await _localStorageService.SetItemAsync("refresh_token_expires", response.RefreshTokenExpiresAt);
                
            // Setup refresh timer for the new token
            SetupRefreshTimer(response.AccessToken);
                
            return true;

        }
        catch
        {
            return false;
        }
    }
    
    private void SetupRefreshTimer(string token)
    {
        // Dispose of existing timer
        _tokenRefreshTimer?.Dispose();
        
        try
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
            
            // Calculate time until token expires minus 1 minute buffer
            var tokenExpiry = jwtToken.ValidTo;
            var timeToExpiry = tokenExpiry - DateTime.UtcNow - TimeSpan.FromMinutes(1);
            
            // Only set up a timer if the token expires in the future
            if (timeToExpiry > TimeSpan.Zero)
            {
                _tokenRefreshTimer = new Timer(async _ => 
                {
                    await TryRefreshToken();
                }, null, timeToExpiry, Timeout.InfiniteTimeSpan);
            }
        }
        catch
        {
            // If we can't parse the token, don't set up a timer
        }
    }
    
    public async Task ClearAuthData()
    {
        await _localStorageService.RemoveItemAsync("token");
        await _localStorageService.RemoveItemAsync("token_expires");
        await _localStorageService.RemoveItemAsync("refresh_token");
        await _localStorageService.RemoveItemAsync("refresh_token_expires");
        
        _tokenRefreshTimer?.Dispose();
        _tokenRefreshTimer = null;
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
}