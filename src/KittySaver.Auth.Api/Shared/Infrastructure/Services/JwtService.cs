using Microsoft.AspNetCore.Identity;

namespace KittySaver.Auth.Api.Shared.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entites;
using Microsoft.IdentityModel.Tokens;

public interface IJwtTokenService
{
    Task<(string token, DateTimeOffset expiresAt)> GenerateTokenAsync(ApplicationUser user);
}

public sealed class JwtTokenService(
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider,
    UserManager<ApplicationUser> userManager)
    : IJwtTokenService
{
    public async Task<(string token, DateTimeOffset expiresAt)> GenerateTokenAsync(ApplicationUser user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);

        List<Claim> claims =
        [
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        ];

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["AppSettings:Token"] ?? throw new Exception("Token not found.")));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        DateTimeOffset expiresAt = dateTimeProvider.Now.AddMinutes(
            int.Parse(configuration["AppSettings:MinutesTokenExpiresIn"]!));

        JwtSecurityToken token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAt.DateTime,
            signingCredentials: credentials);

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwt, expiresAt);
    }
}