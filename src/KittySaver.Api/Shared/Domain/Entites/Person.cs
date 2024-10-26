using System.Text.RegularExpressions;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed partial class Person : AuditableEntity, IContact
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
    private readonly Guid _userIdentityId;
    private readonly List<Cat> _cats = [];
    private readonly List<Advertisement> _advertisements = [];

    public Role CurrentRole { get; private set; } = Role.Regular;

    public required Guid UserIdentityId
    {
        get => _userIdentityId;
        init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Provided empty guid.", nameof(UserIdentityId));
            }
            _userIdentityId = value;   
        }
    }

    public required string FirstName
    {
        get => _firstName;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(FirstName));
            if (value.Length > Constraints.FirstNameMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(FirstName), value,
                    $"Maximum allowed length is: {Constraints.FirstNameMaxLength}");
            }
            string firstChar = value[0].ToString().ToUpper();
            string rest = value[1..].ToLower();
            _firstName = $"{firstChar}{rest}";
        }
    }

    public required string LastName
    {
        get => _lastName;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(LastName));
            if (value.Length > Constraints.LastNameMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(LastName), value,
                    $"Maximum allowed length is: {Constraints.LastNameMaxLength}");
            }
            string firstChar = value[0].ToString().ToUpper();
            string rest = value[1..];
            _lastName = $"{firstChar}{rest}";
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public required string Email
    {
        get => _email;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Email));
            if (value.Length > IContact.Constraints.EmailMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Email), value,
                    $"Maximum allowed length is: {IContact.Constraints.EmailMaxLength}");
            }
            if (!EmailRegex().IsMatch(value))
            {
                throw new FormatException("Provided email is not in correct format.");
            }
            _email = value;
        }
    }

    public required string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(PhoneNumber));
            if (value.Length > IContact.Constraints.PhoneNumberMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(PhoneNumber), value,
                    $"Maximum allowed length is: {IContact.Constraints.PhoneNumberMaxLength}");
            }
            _phoneNumber = value;
        }
    }

    public required Address Address { get; set; }
    public required PickupAddress DefaultPickupAddress { get; set; }
    public required ContactInfo DefaultContactInfo { get; set; }
    
    public IReadOnlyList<Cat> Cats => _cats.ToList();
    public IReadOnlyList<Advertisement> Advertisements => _advertisements.ToList();

    public void AddCat(Cat cat)
    {
        _cats.Add(cat);
    }
    
    public void RemoveCat(Cat cat)
    {
        _cats.Remove(cat);
    }

    public static class Constraints
    {
        public const int FirstNameMaxLength = 50;
        public const int LastNameMaxLength = 50;
    }

    [GeneratedRegex(IContact.Constraints.EmailPattern)]
    private static partial Regex EmailRegex();
}

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");
        
        builder
            .ComplexProperty(x => x.Address)
            .IsRequired();
        
        builder
            .ComplexProperty(x => x.DefaultContactInfo)
            .IsRequired();
        
        builder
            .ComplexProperty(x => x.DefaultPickupAddress)
            .IsRequired();
        
        builder
            .Property(m=>m.FirstName)
            .HasMaxLength(Person.Constraints.FirstNameMaxLength)
            .IsRequired();
        
        builder
            .Property(m=>m.LastName)
            .HasMaxLength(Person.Constraints.LastNameMaxLength)
            .IsRequired();
        
        builder
            .Property(m=>m.Email)
            .HasMaxLength(IContact.Constraints.EmailMaxLength)
            .IsRequired();
        
        builder
            .Property(m=>m.PhoneNumber)
            .HasMaxLength(IContact.Constraints.PhoneNumberMaxLength)
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