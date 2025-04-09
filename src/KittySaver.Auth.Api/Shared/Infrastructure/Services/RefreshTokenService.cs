using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Shared.Infrastructure.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed class RefreshTokenService(
    ApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider) 
    : IRefreshTokenService
{
    private const int RefreshTokenExpirationDays = 7; // Domyślny czas ważności refresh tokena
    
    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string token = Guid.NewGuid().ToString();
        
        RefreshToken refreshToken = new RefreshToken
        {
            Token = token,
            ApplicationUserId = userId,
            ExpiresAt = dateTimeProvider.Now.AddDays(RefreshTokenExpirationDays)
        };
        
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return refreshToken;
    }
    
    public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshToken token = await dbContext.RefreshTokens
            .Include(r => r.ApplicationUser)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken)
            ?? throw new RefreshToken.Exceptions.RefreshTokenNotFoundException();
            
        if (token.RevokedAt is not null)
        {
            throw new RefreshToken.Exceptions.RefreshTokenRevokedException();
        }
            
        if (token.IsExpired)
        {
            throw new RefreshToken.Exceptions.RefreshTokenExpiredException();
        }
            
        return token;
    }
    
    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshToken token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken)
            ?? throw new RefreshToken.Exceptions.RefreshTokenNotFoundException();
            
        token.RevokedAt = dateTimeProvider.Now;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        List<RefreshToken> userRefreshTokens = await dbContext.RefreshTokens
            .Where(r => r.ApplicationUserId == userId && r.RevokedAt == null)
            .ToListAsync(cancellationToken);
            
        foreach (RefreshToken token in userRefreshTokens)
        {
            token.RevokedAt = dateTimeProvider.Now;
        }
            
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}