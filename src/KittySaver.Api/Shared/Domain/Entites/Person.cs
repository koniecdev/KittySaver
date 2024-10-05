using System.Text.RegularExpressions;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed partial class Person : AuditableEntity
{
    public enum Role
    {
        Regular,
        Shelter,
        Admin
    }
    private string _phoneNumber = null!;
    private string _email = null!;
    private string _firstName = null!;
    private string _lastName = null!;

    public Role CurrentRole { get; private set; } = Role.Regular;
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
            string rest = value[1..];
            _lastName = $"{firstChar}{rest}";
        } 
    }

    public string FullName => $"{FirstName} {LastName}";

    public required string Email
    {
        get => _email;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Email));
            if (!EmailRegex().IsMatch(value))
            {
                throw new DomainExceptions.Email.InvalidFormatException();
            }
            _email = value;
        }
    }

    public required string PhoneNumber
    {
        get => _phoneNumber;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(PhoneNumber));
            _phoneNumber = value;
        }
    }

    public ICollection<Cat> Cats { get; } = new List<Cat>();

    public void PromoteToShelter()
    {
        if (CurrentRole is Role.Admin)
        {
            return;
        }
        CurrentRole = Role.Shelter;
    }
    
    public sealed class PersonNotFoundException : NotFoundException
    {
        public PersonNotFoundException(Guid id) : base("Person.NotFound", id.ToString())
        {
        }
        public PersonNotFoundException(string identifier) : base("Person.NotFound", identifier)
        {
        }
    }
    public static class DomainExceptions
    {
        public static class Email
        {
            public sealed class InvalidFormatException() 
                : DomainException("Person.Email.InvalidFormat", "Email format is invalid");
        }
    }

    [GeneratedRegex(ValidationPatterns.EmailPattern)]
    private static partial Regex EmailRegex();
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
            .Property(m=>m.PhoneNumber)
            .HasMaxLength(31)
            .IsRequired();
        builder
            .Property(x=>x.UserIdentityId)
            .IsRequired();
        builder
            .HasIndex(m => m.UserIdentityId)
            .IsUnique();
        builder
            .HasIndex(m => m.Email)
            .IsUnique();
        builder
            .HasIndex(m => m.PhoneNumber)
            .IsUnique();
    }
}