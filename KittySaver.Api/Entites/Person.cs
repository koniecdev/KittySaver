using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using KittySaver.Api.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Entites;

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
                throw new Exceptions.EmptyEmailException();
            }
            if (!Regex.IsMatch(value, EmailPattern))
            {
                throw new Exceptions.InvalidEmailFormatException();
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
                throw new Exceptions.EmptyPhoneNumberException();
            }
            if (!Regex.IsMatch(value, EmailPattern))
            {
                throw new Exceptions.InvalidPhoneNumberFormatException();
            }
            _phoneNumber = value;
        }
    }
    
    private static class Exceptions
    {
        public sealed class EmptyEmailException() : BadRequestException("Email must not be empty");
        public sealed class InvalidEmailFormatException() : BadRequestException("Email is in incorrect format");
        public sealed class EmptyPhoneNumberException() : BadRequestException("Phone number must not be empty");
        public sealed class InvalidPhoneNumberFormatException() : BadRequestException("Phone number is in incorrect format");
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
    }
}