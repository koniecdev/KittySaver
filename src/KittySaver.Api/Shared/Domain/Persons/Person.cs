using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Shared.Domain.Common.Primitives;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons.Events;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Persons;

public sealed class Person : AggregateRoot
{
    private readonly Guid _userIdentityId;
    private readonly List<Cat> _cats = [];

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Person()
    {
        FirstName = null!;
        LastName = null!;
        Email = null!;
        PhoneNumber = null!;
        ResidentalAddress = null!;
        DefaultAdvertisementsPickupAddress = null!;
        DefaultAdvertisementsContactInfoEmail = null!;
        DefaultAdvertisementsContactInfoPhoneNumber = null!;
    }

    private Person(
        Guid userIdentityId,
        FirstName firstName,
        LastName lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address residentalAddress,
        Address defaultAdvertisementPickupAddress,
        Email defaultAdvertisementContactInfoEmail,
        PhoneNumber defaultAdvertisementContactInfoPhone)
    {
        UserIdentityId = userIdentityId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        ResidentalAddress = residentalAddress;
        PhoneNumber = phoneNumber;
        ResidentalAddress = residentalAddress;
        DefaultAdvertisementsPickupAddress = defaultAdvertisementPickupAddress;
        DefaultAdvertisementsContactInfoEmail = defaultAdvertisementContactInfoEmail;
        DefaultAdvertisementsContactInfoPhoneNumber = defaultAdvertisementContactInfoPhone;
    }

    public static Person Create(
        Guid userIdentityId,
        FirstName firstName,
        LastName lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address residentalAddress,
        Address defaultAdvertisementPickupAddress,
        Email defaultAdvertisementContactInfoEmail,
        PhoneNumber defaultAdvertisementContactInfoPhoneNumber)
    {
        Person person = new(
            userIdentityId: userIdentityId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phoneNumber,
            residentalAddress: residentalAddress,
            defaultAdvertisementPickupAddress: defaultAdvertisementPickupAddress,
            defaultAdvertisementContactInfoEmail: defaultAdvertisementContactInfoEmail,
            defaultAdvertisementContactInfoPhone: defaultAdvertisementContactInfoPhoneNumber
        );
        return person;
    }

    public Role CurrentRole { get; private init; } = Role.Regular;
    public FirstName FirstName { get; set; }
    public LastName LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public Email Email { get; set; }
    public PhoneNumber PhoneNumber { get; set; }
    public Address ResidentalAddress { get; set; }
    public Address DefaultAdvertisementsPickupAddress { get; set; }
    public Email DefaultAdvertisementsContactInfoEmail { get; set; }
    public PhoneNumber DefaultAdvertisementsContactInfoPhoneNumber { get; set; }

    public Guid UserIdentityId
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

    public IReadOnlyList<Cat> Cats => _cats.ToList();

    public void AddCat(Cat cat)
    {
        if (_cats.Count > 0 && _cats.Any(c => c.Id == cat.Id))
        {
            throw new InvalidOperationException($"Cat with id: '{cat.Id}' is already assigned to Person with id {Id}.");
        }

        _cats.Add(cat);
    }

    public void RemoveCat(Cat cat)
    {
        switch (_cats.Count)
        {
            case 0:
            case > 0 when _cats.All(c => c.Id != cat.Id):
                throw new InvalidOperationException($"Cat with id: '{cat.Id}' is not even assigned to Person with id {Id}.");
            default:
                if (cat.AdvertisementId is not null)
                {
                    throw new InvalidOperationException("Can not delete cat, that is assigned to advertisement.");
                }
                _cats.Remove(cat);
                break;
        }
    }

    public void AssignCatToDraftAdvertisement(Guid advertisementId, Guid catId)
    {
        Cat cat = Cats.FirstOrDefault(c => c.Id == catId)
                  ?? throw new NotFoundExceptions.CatNotFoundException(catId);
        
        cat.AssignAdvertisement(advertisementId);
    }
    
    public void AssignCatToActiveAdvertisement(Guid advertisementId, Guid catId)
    {
        Cat cat = Cats.FirstOrDefault(c => c.Id == catId)
                  ?? throw new NotFoundExceptions.CatNotFoundException(catId);
        
        cat.AssignAdvertisement(advertisementId);
        
        RaiseDomainEvent(new AssignedToAdvertisementCatStatusChangedDomainEvent(cat.AdvertisementId!.Value));
    }

    public void UnassignCatFromAdvertisement(Guid catId)
    {
        Cat cat = Cats.FirstOrDefault(c => c.Id == catId)
                  ?? throw new NotFoundExceptions.CatNotFoundException(catId);
        
        Guid? unassignedAdvertisementId = cat.AdvertisementId;
        
        cat.UnassignAdvertisement();
        
        RaiseDomainEvent(new AssignedToAdvertisementCatStatusChangedDomainEvent(unassignedAdvertisementId!.Value));
    }

    public void ReplaceCatPriorityCompounds(
        ICatPriorityCalculatorService catPriorityCalculator,
        Guid catId,
        HealthStatus healthStatus,
        AgeCategory ageCategory,
        Behavior behavior,
        MedicalHelpUrgency medicalHelpUrgency)
    {
        Cat cat = Cats.First(c => c.Id == catId);
        cat.HealthStatus = healthStatus;
        cat.AgeCategory = ageCategory;
        cat.Behavior = behavior;
        cat.MedicalHelpUrgency = medicalHelpUrgency;
        cat.RecalculatePriorityScore(catPriorityCalculator);
        if (cat.AdvertisementId is not null)
        {
            RaiseDomainEvent(new AssignedToAdvertisementCatStatusChangedDomainEvent(cat.AdvertisementId.Value));
        }
    }

    public double GetHighestPriorityScoreFromGivenCats(IEnumerable<Guid> catsIds)
    {
        List<Guid> catsIdsList = catsIds.ToList();

        HashSet<Guid> catIdsFromPersonCats = Cats.Select(cat => cat.Id).ToHashSet();
        if (!catsIdsList.All(catId => catIdsFromPersonCats.Contains(catId)))
        {
            throw new ArgumentException("One or more provided cats do not belong to provided person.", nameof(catsIds));
        }

        double highestPriorityScore = Cats
            .Where(cat => catsIdsList.Contains(cat.Id))
            .Max(cat => cat.PriorityScore);

        return highestPriorityScore;
    }

    public void AnnounceDeletion()
    {
        RaiseDomainEvent(new PersonDeletedDomainEvent(Id));
    }

    public enum Role
    {
        Regular,
        Admin
    }
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

        builder.HasMany<Advertisement.Advertisement>()
            .WithOne()
            .HasForeignKey(advertisement => advertisement.PersonId)
            .IsRequired();

        builder.ComplexProperty(x => x.ResidentalAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Country)
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.State)
                .HasMaxLength(Address.StateMaxLength);

            complexPropertyBuilder
                .Property(x => x.ZipCode)
                .HasMaxLength(Address.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.City)
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.Street)
                .HasMaxLength(Address.StreetMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.BuildingNumber)
                .HasMaxLength(Address.BuildingNumberMaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsPickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder
                .Property(x => x.Country)
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.State)
                .HasMaxLength(Address.StateMaxLength);

            complexPropertyBuilder
                .Property(x => x.ZipCode)
                .HasMaxLength(Address.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.City)
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder
                .Property(x => x.Street)
                .HasMaxLength(Address.StreetMaxLength);

            complexPropertyBuilder
                .Property(x => x.BuildingNumber)
                .HasMaxLength(Address.BuildingNumberMaxLength);
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfoEmail, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder
                .Property(x => x.Value)
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfoPhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder
                .Property(x => x.Value)
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.FirstName, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(FirstName.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.LastName, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(LastName.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Email, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.PhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.Property(x => x.UserIdentityId)
            .IsRequired();

        builder.Ignore(x => x.FullName);
    }
}