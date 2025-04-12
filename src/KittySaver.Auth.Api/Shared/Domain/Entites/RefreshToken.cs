using KittySaver.Auth.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Auth.Api.Shared.Domain.Entites;

public sealed class RefreshToken : AuditableEntity
{
    public required string Token { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool IsExpired => DateTimeOffset.Now >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
    
    public required Guid ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    
    public static class Exceptions
    {
        public sealed class RefreshTokenExpiredException()
            : InvalidOperationException("Refresh token has expired");
            
        public sealed class RefreshTokenRevokedException()
            : InvalidOperationException("Refresh token has been revoked");
            
        public sealed class RefreshTokenNotFoundException()
            : NotFoundException("RefreshToken.NotFound", "Refresh token not found");
    }
}

internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder
            .Property(m => m.Token)
            .HasMaxLength(128)
            .IsRequired();
            
        builder
            .HasIndex(m => m.Token)
            .IsUnique();
            
        builder
            .HasOne(m => m.ApplicationUser)
            .WithMany()
            .HasForeignKey(m => m.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}