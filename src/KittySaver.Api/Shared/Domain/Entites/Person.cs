using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed partial class Person : AuditableEntity, IContact
{
    public static Person Create(
        Guid userIdentityId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        Address address,
        PickupAddress defaultAdvertisementPickupAddress,
        ContactInfo defaultAdvertisementContactInfo)
    {
        Person person = new(
            userIdentityId: userIdentityId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phoneNumber,
            address: address,
            defaultAdvertisementPickupAddress: defaultAdvertisementPickupAddress,
            defaultAdvertisementContactInfo: defaultAdvertisementContactInfo
        );
        return person;
    }

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Person()
    {
    }

    [SetsRequiredMembers]
    private Person(
        Guid userIdentityId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        Address address,
        PickupAddress defaultAdvertisementPickupAddress,
        ContactInfo defaultAdvertisementContactInfo)
    {
        UserIdentityId = userIdentityId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        DefaultAdvertisementsPickupAddress = defaultAdvertisementPickupAddress;
        DefaultAdvertisementsContactInfo = defaultAdvertisementContactInfo;
    }

    private string _phoneNumber = null!;
    private string _email = null!;
    private string _firstName = null!;
    private string _lastName = null!;
    private readonly Guid _userIdentityId;
    private readonly List<Cat> _cats = [];

    public Role CurrentRole { get; private init; } = Role.Regular;

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
    public required PickupAddress DefaultAdvertisementsPickupAddress { get; set; }
    public required ContactInfo DefaultAdvertisementsContactInfo { get; set; }

    public IReadOnlyList<Cat> Cats => _cats.ToList();

    public void AddCat(Cat cat)
    {
        if (_cats.Count > 0 && _cats.Any(c => c.Id == cat.Id))
        {
            return;
        }
        _cats.Add(cat);
    }

    public void RemoveCat(Cat cat)
    {
        switch (_cats.Count)
        {
            case 0:
            case > 0 when _cats.All(c => c.Id != cat.Id):
                return;
            default:
                _cats.Remove(cat);
                break;
        }
    }
    
    public static class Constraints
    {
        public const int FirstNameMaxLength = 50;
        public const int LastNameMaxLength = 50;
    }

    public enum Role
    {
        Regular,
        Admin
    }

    [GeneratedRegex(IContact.Constraints.EmailPattern)]
    private static partial Regex EmailRegex();
}

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");
        
        builder.Property(person => person.Id).ValueGeneratedNever();
        
        builder.HasKey(person => person.Id);

        builder.HasMany(person => person.Cats)
            .WithOne()
            .HasForeignKey(cat => cat.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasMany<Advertisement>()
            .WithOne()
            .HasForeignKey(advertisement => advertisement.PersonId)
            .OnDelete(DeleteBehavior.Cascade) //TODO: Should be refactored to Domain Events
            .IsRequired();

        builder.ComplexProperty(x => x.Address, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();
            
            complexPropertyBuilder.Property(x => x.Country)
                .HasMaxLength(IAddress.Constraints.CountryMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.State)
                .HasMaxLength(IAddress.Constraints.StateMaxLength);
            
            complexPropertyBuilder
                .Property(x => x.ZipCode)
                .HasMaxLength(IAddress.Constraints.ZipCodeMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.City)
                .HasMaxLength(IAddress.Constraints.CityMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.Street)
                .HasMaxLength(IAddress.Constraints.StreetMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.BuildingNumber)
                .HasMaxLength(IAddress.Constraints.BuildingNumberMaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsPickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.Country)
                .HasMaxLength(IAddress.Constraints.CountryMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.State)
                .HasMaxLength(IAddress.Constraints.StateMaxLength);
            
            complexPropertyBuilder
                .Property(x => x.ZipCode)
                .HasMaxLength(IAddress.Constraints.ZipCodeMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.City)
                .HasMaxLength(IAddress.Constraints.CityMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.Street)
                .HasMaxLength(IAddress.Constraints.StreetMaxLength);
            
            complexPropertyBuilder
                .Property(x => x.BuildingNumber)
                .HasMaxLength(IAddress.Constraints.BuildingNumberMaxLength);
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfo, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.Email)
                .HasMaxLength(IContact.Constraints.EmailMaxLength)
                .IsRequired();
            
            complexPropertyBuilder
                .Property(x => x.PhoneNumber)
                .HasMaxLength(IContact.Constraints.PhoneNumberMaxLength);
        });

        builder.Property(m => m.FirstName)
            .HasMaxLength(Person.Constraints.FirstNameMaxLength)
            .IsRequired();

        builder.Property(m => m.LastName)
            .HasMaxLength(Person.Constraints.LastNameMaxLength)
            .IsRequired();

        builder.Property(m => m.Email)
            .HasMaxLength(IContact.Constraints.EmailMaxLength)
            .IsRequired();

        builder.Property(m => m.PhoneNumber)
            .HasMaxLength(IContact.Constraints.PhoneNumberMaxLength)
            .IsRequired();

        builder.Property(x => x.UserIdentityId)
            .IsRequired();

        builder.HasIndex(m => m.UserIdentityId)
            .IsUnique();

        builder.HasIndex(m => m.Email)
            .IsUnique();

        builder.HasIndex(m => m.PhoneNumber)
            .IsUnique();
    }
}