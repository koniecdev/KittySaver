using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons.Events;
using KittySaver.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Domain.Persons;

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

    public enum Role
    {
        Regular,
        Admin
    }
    
    public Role CurrentRole { get; private init; } = Role.Regular;
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address ResidentalAddress { get; private set; }
    public Address DefaultAdvertisementsPickupAddress { get; private set; }
    public Email DefaultAdvertisementsContactInfoEmail { get; private set; }
    public PhoneNumber DefaultAdvertisementsContactInfoPhoneNumber { get; private set; }

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

    public void ChangeName(FirstName firstName, LastName lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangeEmail(Email email)
    {
        Email = email;
    }
    
    public void ChangePhoneNumber(PhoneNumber phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
    
    public void ChangeResidentalAddress(Address residentalAddress)
    {
        ResidentalAddress = residentalAddress;
    }
    
    public void ChangeDefaultsForAdvertisement(
        Address defaultAdvertisementsPickupAddress,
        Email defaultAdvertisementsContactInfoEmail,
        PhoneNumber defaultAdvertisementsContactInfoPhoneNumber)
    {
        DefaultAdvertisementsPickupAddress = defaultAdvertisementsPickupAddress;
        DefaultAdvertisementsContactInfoEmail = defaultAdvertisementsContactInfoEmail;
        DefaultAdvertisementsContactInfoPhoneNumber = defaultAdvertisementsContactInfoPhoneNumber;
    }
    
    public IEnumerable<Guid> GetAssignedToConcreteAdvertisementCatIds(Guid advertisementId)
    {
        List<Guid> cats = Cats
            .Where(x => x.AdvertisementId == advertisementId)
            .Select(x=>x.Id)
            .ToList();
        return cats;
    }

    public void AddCat(Cat cat)
    {
        ThrowIfCatExists(cat.Id);
        _cats.Add(cat);
    }

    public void RemoveCat(Guid catId)
    {
        Cat cat = GetCatById(catId);
        if (cat.AdvertisementId.HasValue)
        {
            ThrowIfCatNotExists(cat.Id);
        }
        _cats.Remove(cat);
    }
    
    public void AssignCatToAdvertisement(Guid advertisementId, Guid catId)
    {
        Cat cat = GetCatById(catId);
        cat.AssignAdvertisement(advertisementId);
    }

    public void UnassignCatFromAdvertisement(Guid catId)
    {
        Cat cat = GetCatById(catId);
        Guid? unassignedAdvertisementId = cat.AdvertisementId;
        
        cat.UnassignAdvertisement();
        
        if (unassignedAdvertisementId.HasValue)
        {
            RaiseDomainEvent(new AssignedToAdvertisementCatStatusChangedDomainEvent(unassignedAdvertisementId.Value));
        }
    }
    
    public void UnassignCatsFromRemovedAdvertisement(Guid advertisementId)
    {
        IEnumerable<Cat> catsQuery = GetAssignedToConcreteAdvertisementCats(advertisementId);
        foreach (Cat cat in catsQuery)
        {
            cat.UnassignAdvertisement();
        }
    }

    public void MarkCatsFromConcreteAdvertisementAsAdopted(Guid advertisementId)
    {
        IEnumerable<Cat> catsQuery = GetAssignedToConcreteAdvertisementCats(advertisementId);
        foreach (Cat cat in catsQuery)
        {
            cat.MarkAsAdopted();
        }
    }
    
    public void ReplaceCatPriorityCompounds(
        ICatPriorityCalculatorService catPriorityCalculator,
        Guid catId,
        HealthStatus healthStatus,
        AgeCategory ageCategory,
        Behavior behavior,
        MedicalHelpUrgency medicalHelpUrgency)
    {
        Cat cat = GetCatById(catId);
        cat.HealthStatus = healthStatus;
        cat.AgeCategory = ageCategory;
        cat.Behavior = behavior;
        cat.MedicalHelpUrgency = medicalHelpUrgency;
        cat.RecalculatePriorityScore(catPriorityCalculator);
        
        if (cat.AdvertisementId.HasValue)
        {
            RaiseDomainEvent(new AssignedToAdvertisementCatStatusChangedDomainEvent(cat.AdvertisementId.Value));
        }
    }
    
    public double GetHighestPriorityScoreFromGivenCats(IEnumerable<Guid> catsIds)
    {
        List<Guid> catsIdsList = catsIds.ToList();

        ValidateCatsOwnership(catsIdsList);

        double highestPriorityScore = Cats
            .Where(cat => catsIdsList.Contains(cat.Id))
            .Max(cat => cat.PriorityScore);

        return highestPriorityScore;
    }

    public void AnnounceDeletion() => RaiseDomainEvent(new PersonDeletedDomainEvent(Id));

    private IEnumerable<Cat> GetAssignedToConcreteAdvertisementCats(Guid advertisementId) 
        => _cats.Where(x => x.AdvertisementId == advertisementId);
    
    private void ValidateCatsOwnership(IEnumerable<Guid> catIds)
    {
        HashSet<Guid> catIdsFromPersonCats = Cats.Select(cat => cat.Id).ToHashSet();
        if (!catIds.All(catId => catIdsFromPersonCats.Contains(catId)))
        {
            throw new ArgumentException(ErrorMessages.InvalidCatsOwnership, nameof(catIds));
        }
    }
    
    private void ThrowIfCatExists(Guid catId)
    {
        if (_cats.Any(c => c.Id == catId))
        {
            throw new InvalidOperationException(string.Format(ErrorMessages.CatAlreadyAssigned, catId, Id));
        }
    }

    private void ThrowIfCatNotExists(Guid catId)
    {
        if (_cats.All(c => c.Id != catId))
        {
            throw new InvalidOperationException(string.Format(ErrorMessages.CatNotAssigned, catId, Id));
        }
    }
    
    private Cat GetCatById(Guid catId) =>
        _cats.FirstOrDefault(c => c.Id == catId) ?? 
        throw new NotFoundExceptions.CatNotFoundException(catId);
    
    private static class ErrorMessages
    {
        public const string CatAlreadyAssigned = "Cat with id: '{0}' is already assigned to Person with id {1}.";
        public const string CatNotAssigned = "Cat with id: '{0}' is not even assigned to Person with id {1}.";
        public const string InvalidCatsOwnership = "One or more provided cats do not belong to provided person.";
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

        builder.HasMany<Advertisement>()
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