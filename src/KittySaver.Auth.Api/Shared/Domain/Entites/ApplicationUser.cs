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
    private string _defaultAdvertisementPickupAddressCountry;
    private string? _defaultAdvertisementPickupAddressState;
    private string _defaultAdvertisementPickupAddressZipCode;
    private string _defaultAdvertisementPickupAddressCity;
    private string _defaultAdvertisementPickupAddressStreet;
    private string _defaultAdvertisementPickupAddressBuildingNumber;
    private string _defaultAdvertisementContactInfoEmail;
    private string _defaultAdvertisementContactInfoPhoneNumber;

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

    public required string DefaultAdvertisementPickupAddressCountry
    {
        get => _defaultAdvertisementPickupAddressCountry;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementPickupAddressCountry));
            _defaultAdvertisementPickupAddressCountry = value;   
        }
    }

    public required string? DefaultAdvertisementPickupAddressState
    {
        get => _defaultAdvertisementPickupAddressState;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementPickupAddressState));

            _defaultAdvertisementPickupAddressState = value;   
        }
    }

    public required string DefaultAdvertisementPickupAddressZipCode
    {
        get => _defaultAdvertisementPickupAddressZipCode;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementPickupAddressZipCode));
            _defaultAdvertisementPickupAddressZipCode = value;
        }
    }

    public required string DefaultAdvertisementPickupAddressCity
    {
        get => _defaultAdvertisementPickupAddressCity;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementPickupAddressCity));
            _defaultAdvertisementPickupAddressCity = value;
        }
    }

    public required string DefaultAdvertisementPickupAddressStreet
    {
        get => _defaultAdvertisementPickupAddressStreet;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementPickupAddressStreet));
            _defaultAdvertisementPickupAddressStreet = value;
        }
    }

    public required string DefaultAdvertisementPickupAddressBuildingNumber
    {
        get => _defaultAdvertisementPickupAddressBuildingNumber;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementPickupAddressBuildingNumber));
            _defaultAdvertisementPickupAddressBuildingNumber = value;
        }
    }

    public required string DefaultAdvertisementContactInfoEmail
    {
        get => _defaultAdvertisementContactInfoEmail;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementContactInfoEmail));
            _defaultAdvertisementContactInfoEmail = value;
        }
    }

    public required string DefaultAdvertisementContactInfoPhoneNumber
    {
        get => _defaultAdvertisementContactInfoPhoneNumber;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DefaultAdvertisementContactInfoPhoneNumber));
            _defaultAdvertisementContactInfoPhoneNumber = value;
        }
    }

    public static class Exceptions
    {
        public sealed class ApplicationUserNotFoundException()
            : NotFoundException("ApplicationUser.Email.Empty", "Email is empty");

        public static class Email
        {
            public sealed class InvalidFormatException()
                : BadRequestException("ApplicationUser.Email.InvalidFormat", "Email format is invalid");

            public sealed class NotUniqueException()
                : BadRequestException("ApplicationUser.Email.NotUnique", "Email is not unique");
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
            .Property(m => m.DefaultAdvertisementPickupAddressCountry)
            .HasMaxLength(100)
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementPickupAddressState)
            .HasMaxLength(100)
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementPickupAddressZipCode)
            .HasMaxLength(20)
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementPickupAddressCity)
            .HasMaxLength(100)
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementPickupAddressStreet)
            .HasMaxLength(200)
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementPickupAddressBuildingNumber)
            .HasMaxLength(20)
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementContactInfoEmail)
            .HasMaxLength(254)  // Same as the main Email field
            .IsRequired();
    
        builder
            .Property(m => m.DefaultAdvertisementContactInfoPhoneNumber)
            .HasMaxLength(31)   // Same as the main PhoneNumber field
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