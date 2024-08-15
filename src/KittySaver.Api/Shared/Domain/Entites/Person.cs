using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using KittySaver.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Person : IdentityUser<Guid>
{
    private static readonly string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    private string? _userName;
    private string? _email;
    private string? _phoneNumber;
    private string _firstName = null!;
    private string _lastName = null!;

    public required Guid UserIdentityId { get; init; }
    public required string FirstName
    {
        get => _firstName;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(FirstName));
            string firstChar = value[0].ToString().ToUpper();
            string rest = value[1..].ToLower();
            _firstName = $"{firstChar}{rest}";
        } 
    }

    public required string LastName
    {
        get => _lastName;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(LastName));
            string firstChar = value[0].ToString().ToUpper();
            string rest = value[1..].ToLower();
            _lastName = $"{firstChar}{rest}";
        } 
    }

    public string FullName => $"{FirstName} {LastName}";
    public override string? UserName
    {
        get => _userName;
        set 
        {
            if (value != Email)
            {
                _userName = Email;
            }
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
            _userName = value;
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
        public sealed class PersonNotFoundException() 
            : NotFoundException("Person.Email.Empty", "Email is empty");
        
        public static class Email
        {
            public sealed class InvalidFormatException() 
                : BadRequestException("Person.Email.InvalidFormat", "Email format is invalid");
            public sealed class NotUniqueException() 
                : BadRequestException("Person.Email.NotUnique", "Email is not unique");
        }
    }
}

internal class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder
            .Property(m=>m.FirstName)
            .HasMaxLength(50)
            .IsRequired();
        builder
            .Property(m=>m.LastName)
            .HasMaxLength(50)
            .IsRequired();
        builder
            .Property(m=>m.Email)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m => m.NormalizedEmail)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m=>m.UserName)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m => m.NormalizedUserName)
            .HasMaxLength(254)
            .IsRequired();
        builder
            .Property(m=>m.PasswordHash)
            .IsRequired();
        builder
            .Property(m=>m.PhoneNumber)
            .HasMaxLength(31)
            .IsRequired();

        builder
            .HasIndex(m => m.UserIdentityId)
            .IsUnique();
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