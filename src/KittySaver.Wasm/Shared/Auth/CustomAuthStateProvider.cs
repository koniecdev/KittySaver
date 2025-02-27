using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace KittySaver.Wasm.Shared.Auth;

public class CustomAuthStateProvider(ILocalStorageService localStorageService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await localStorageService.GetItemAsStringAsync("token");
        ClaimsIdentity identity = new();
    
        if (!string.IsNullOrWhiteSpace(token))
        {
            token = token.Replace("\"", "");
        
            bool isTokenExpired = IsTokenExpired(token);
        
            if (!isTokenExpired)
            {
                IEnumerable<Claim> claims = ParseClaimsFromJwt(token);
                identity = new ClaimsIdentity(claims, "jwt");
            }
            else
            {
                await localStorageService.RemoveItemAsync("token");
                await localStorageService.RemoveItemAsync("token_expires");
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
}