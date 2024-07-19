using System.Text.RegularExpressions;
using KittySaver.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public class Person : IdentityUser<Guid>
{
    private static readonly string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private static readonly string PhonePattern = @"^\+?[1-9]\d{1,14}$";
    private string? _userName;
    private string? _email;
    private string? _phoneNumber;

    public override required string? UserName
    {
        get => _userName;
        set {
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
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exceptions.Email.EmptyException();
            }
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
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exceptions.PhoneNumber.EmptyException();
            }
            if (!Regex.IsMatch(value, EmailPattern))
            {
                throw new Exceptions.PhoneNumber.InvalidFormatException();
            }
            _phoneNumber = value;
        }
    }
    
    public static class Exceptions
    {
        public sealed class PersonNotFoundException() 
            : NotFoundException("Person.Email.Empty", "Email is empty");
        
        public static class Email
        {
            public sealed class EmptyException() 
                : BadRequestException("Person.Email.Empty", "Email is empty");
            public sealed class InvalidFormatException() 
                : BadRequestException("Person.Email.InvalidFormat", "Email format is invalid");
            public sealed class NotUniqueException() 
                : BadRequestException("Person.Email.NotUnique", "Email is not unique");
        }

        public static class PhoneNumber
        {
            public sealed class EmptyException() 
                : BadRequestException("Person.PhoneNumber.Empty", "Phone number is empty");
            public sealed class InvalidFormatException() 
                : BadRequestException("Person.PhoneNumber.InvalidFormat", "Phone number is invalid");
        }
    }
}

internal class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
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