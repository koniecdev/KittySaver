using System.Text.RegularExpressions;
using KittySaver.Auth.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Auth.Api.Shared.Domain.Entites;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    private static readonly string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    private string? _userName;
    private string? _email;
    private string? _phoneNumber;

    public override required string? UserName
    {
        get => _userName;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(UserName));
            _userName = value;
        }
    }

    public override required string? Email
    {
        get => _email;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Email));
            if (!Regex.IsMatch(value, EmailPattern))
            {
                throw new Exceptions.Email.InvalidFormatException();
            }

            _email = value;
        }
    }

    public override required string? PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(PhoneNumber));
            _phoneNumber = value;
        }
    }

    public static class Exceptions
    {
        public sealed class ApplicationUserNotFoundException()
            : NotFoundException("ApplicationUser.Email.Empty", "Email is empty");

        public static class Email
        {
            public sealed class InvalidFormatException()
                : FormatException("Email format is invalid");
        }
    }
}

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .Property(m => m.UserName)
            .HasMaxLength(50)
            .IsRequired();
        builder
            .Property(m => m.Email)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m => m.NormalizedEmail)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m => m.NormalizedUserName)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m => m.PasswordHash)
            .IsRequired();
        builder
            .Property(m => m.PhoneNumber)
            .HasMaxLength(31)
            .IsRequired();
        
        builder
            .HasIndex(m => m.Email)
            .IsUnique();
        builder
            .HasIndex(m => m.NormalizedEmail)
            .IsUnique();
        builder
            .HasIndex(m => m.UserName)
            .IsUnique();
        builder
            .HasIndex(m => m.NormalizedUserName)
            .IsUnique();
    }
}