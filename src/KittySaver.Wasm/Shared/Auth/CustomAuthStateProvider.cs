using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using KittySaver.Wasm.Shared.HttpClients;
using Microsoft.AspNetCore.Components.Authorization;

namespace KittySaver.Wasm.Shared.Auth;

public class CustomAuthStateProvider(ILocalStorageService localStorageService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await localStorageService.GetItemAsStringAsync("token");
        ClaimsIdentity identity = new ClaimsIdentity();
        if (!string.IsNullOrWhiteSpace(token))
        {
            token = token.Replace("\"", "");
            IEnumerable<Claim> claims = ParseClaimsFromJwt(token);
            identity = new ClaimsIdentity(claims, "jwt");
        }
        
        ClaimsPrincipal user = new(identity);
        AuthenticationState state = new(user);
        
        NotifyAuthenticationStateChanged(Task.FromResult(state));

        return state;
    }
    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}