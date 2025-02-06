using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Domain.Persons;

public sealed class Person : AggregateRoot
{
    private Guid _userIdentityId;
    private readonly List<Cat> _cats = [];
    private readonly List<Advertisement> _advertisements = [];

    public enum Role
    {
        Regular,
        Admin,
        Job,
        Shelter
    }

    public Role CurrentRole { get; private init; } = Role.Regular;

    public Guid UserIdentityId
    {
        get => _userIdentityId;
        private set
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Provided empty guid.", nameof(UserIdentityId));
            }

            _userIdentityId = value;
        }
    }

    public Nickname Nickname { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address DefaultAdvertisementsPickupAddress { get; private set; }
    public Email DefaultAdvertisementsContactInfoEmail { get; private set; }
    public PhoneNumber DefaultAdvertisementsContactInfoPhoneNumber { get; private set; }
    public IReadOnlyList<Cat> Cats => _cats.ToList();
    public IReadOnlyList<Advertisement> Advertisements => _advertisements.ToList();

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Person()
    {
        Nickname = null!;
        Email = null!;
        PhoneNumber = null!;
        DefaultAdvertisementsPickupAddress = null!;
        DefaultAdvertisementsContactInfoEmail = null!;
        DefaultAdvertisementsContactInfoPhoneNumber = null!;
    }

    private Person(
        Nickname nickname,
        Email email,
        PhoneNumber phoneNumber,
        Address defaultAdvertisementPickupAddress,
        Email defaultAdvertisementContactInfoEmail,
        PhoneNumber defaultAdvertisementContactInfoPhone)
    {
        Nickname = nickname;
        Email = email;
        PhoneNumber = phoneNumber;
        DefaultAdvertisementsPickupAddress = defaultAdvertisementPickupAddress;
        DefaultAdvertisementsContactInfoEmail = defaultAdvertisementContactInfoEmail;
        DefaultAdvertisementsContactInfoPhoneNumber = defaultAdvertisementContactInfoPhone;
    }

    public static Person Create(
        Nickname nickname,
        Email email,
        PhoneNumber phoneNumber,
        Address defaultAdvertisementPickupAddress,
        Email defaultAdvertisementContactInfoEmail,
        PhoneNumber defaultAdvertisementContactInfoPhoneNumber)
    {
        Person person = new(
            nickname: nickname,
            email: email,
            phoneNumber: phoneNumber,
            defaultAdvertisementPickupAddress: defaultAdvertisementPickupAddress,
            defaultAdvertisementContactInfoEmail: defaultAdvertisementContactInfoEmail,
            defaultAdvertisementContactInfoPhone: defaultAdvertisementContactInfoPhoneNumber
        );
        return person;
    }

    public Cat AddCat(
        ICatPriorityCalculatorService priorityScoreCalculator,
        CatName name,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus,
        Description additionalRequirements,
        bool isCastrated = false)
    {
        Cat cat = Cat.Create(
            priorityScoreCalculator,
            Id,
            name,
            medicalHelpUrgency,
            ageCategory,
            behavior,
            healthStatus,
            additionalRequirements,
            isCastrated);
        _cats.Add(cat);
        return cat;
    }

    public void RemoveCat(Guid catId)
    {
        Cat cat = GetCatById(catId);
        ThrowIfCatIsAssignedToAdvertisement(cat);
        _cats.Remove(cat);
    }

    public void UpdateCat(
        Guid catId,
        ICatPriorityCalculatorService catPriorityCalculator,
        CatName name,
        Description additionalRequirements,
        bool isCastrated,
        HealthStatus healthStatus,
        AgeCategory ageCategory,
        Behavior behavior,
        MedicalHelpUrgency medicalHelpUrgency)
    {
        Cat catToUpdate = GetCatById(catId);
        catToUpdate.ChangeName(name);
        catToUpdate.ChangeAdditionalRequirements(additionalRequirements);
        catToUpdate.ChangeIsCastratedFlag(isCastrated);
        catToUpdate.ChangePriorityCompounds(catPriorityCalculator, healthStatus, ageCategory, behavior,
            medicalHelpUrgency);

        if (catToUpdate.AdvertisementId.HasValue)
        {
            UpdateAdvertisementPriorityScore(catToUpdate.AdvertisementId.Value);
        }
    }

    public Advertisement AddAdvertisement(
        DateTimeOffset dateOfCreation,
        IEnumerable<Guid> catsIdsToAssign,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber,
        Description description)
    {
        List<Guid> catsIdsToAssignList = catsIdsToAssign.ToList();
        if (catsIdsToAssignList.Count == 0)
        {
            throw new ArgumentException(ErrorMessages.EmptyCatsList, nameof(catsIdsToAssign));
        }
        
        double catsToAssignToAdvertisementHighestPriorityScore =
            GetHighestPriorityScoreFromGivenCats(catsIdsToAssignList);
        
        Advertisement advertisement = Advertisement.Create(
            dateOfCreation,
            Id,
            CurrentRole,
            pickupAddress,
            contactInfoEmail,
            contactInfoPhoneNumber,
            description,
            catsToAssignToAdvertisementHighestPriorityScore);
        
        foreach (Guid catId in catsIdsToAssignList)
        {
            AssignCatToAdvertisement(advertisement.Id, catId);
        }
        
        _advertisements.Add(advertisement);
        
        return advertisement;
    }

    public void RemoveAdvertisement(Guid advertisementId)
    {
        Advertisement advertisement = GetAdvertisementById(advertisementId);
        _advertisements.Remove(advertisement);
        IEnumerable<Cat> catsOfAdvertisementQuery = GetAssignedToConcreteAdvertisementCats(advertisement.Id);
        foreach (Cat cat in catsOfAdvertisementQuery)
        {
            cat.UnassignAdvertisement();
        }
    }

    public void UpdateAdvertisement(
        Guid advertisementId,
        Description description,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber)
    {
        Advertisement advertisementToUpdate = GetAdvertisementById(advertisementId);
        advertisementToUpdate.ChangeDescription(description);
        advertisementToUpdate.ChangePickupAddress(pickupAddress);
        advertisementToUpdate.ChangeContactInfo(contactInfoEmail, contactInfoPhoneNumber);
    }

    public void CloseAdvertisement(Guid advertisementId, DateTimeOffset currentDate)
    {
        Advertisement advertisement = GetAdvertisementById(advertisementId);
        IEnumerable<Cat> catsOfClosedAdvertisementQuery = GetAssignedToConcreteAdvertisementCats(advertisementId);
        advertisement.Close(currentDate);
        foreach (Cat cat in catsOfClosedAdvertisementQuery)
        {
            cat.MarkAsAdopted();
        }
    }

    public void ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(Guid advertisementId)
    {
        Advertisement advertisement = GetAdvertisementById(advertisementId);
        if (advertisement.Status is Advertisement.AdvertisementStatus.ThumbnailNotUploaded)
        {
            advertisement.Activate();
        }
    }
    
    public void ExpireAdvertisement(Guid advertisementId, DateTimeOffset currentDate)
    {
        Advertisement advertisement = GetAdvertisementById(advertisementId);
        advertisement.Expire(currentDate);
    }

    public void RefreshAdvertisement(Guid advertisementId, DateTimeOffset currentDate)
    {
        Advertisement advertisement = GetAdvertisementById(advertisementId);
        advertisement.Refresh(currentDate);
    }

    public void ReplaceCatsOfAdvertisement(Guid advertisementId, IEnumerable<Guid> catsToAssignIds)
    {
        IEnumerable<Guid> catsIdsQuery = Cats
            .Where(x => x.AdvertisementId == advertisementId)
            .Select(x => x.Id);
        
        foreach (Guid catId in catsIdsQuery)
        {
            UnassignCatFromAdvertisement(catId);
        }

        foreach (Guid catId in catsToAssignIds)
        {
            AssignCatToAdvertisement(advertisementId, catId);
        }

        UpdateAdvertisementPriorityScore(advertisementId);
    }

    public void ChangeNickname(Nickname nickname)
    {
        Nickname = nickname;
    }

    public void ChangeEmail(Email email)
    {
        Email = email;
    }

    public void ChangePhoneNumber(PhoneNumber phoneNumber)
    {
        PhoneNumber = phoneNumber;
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

    public double GetHighestPriorityScoreFromGivenCats(IEnumerable<Guid> catsIds)
    {
        List<Guid> catsIdsList = catsIds.ToList();

        ValidateCatsOwnership(catsIdsList);

        double highestPriorityScore = Cats
            .Where(cat => catsIdsList.Contains(cat.Id))
            .Max(cat => cat.PriorityScore);

        return highestPriorityScore;
    }

    public void MarkCatAsThumbnailUploaded(Guid catId)
    {
        Cat cat = GetCatById(catId);
        cat.MarkAsThumbnailUploaded();
    }
    
    public void SetUserIdentityId(Guid userIdentityId)
    {
        UserIdentityId = userIdentityId;
    }
    
    private void AssignCatToAdvertisement(Guid advertisementId, Guid catId)
    {
        Cat cat = GetCatById(catId);
        cat.AssignAdvertisement(advertisementId);
    }

    private void UnassignCatFromAdvertisement(Guid catId)
    {
        Cat cat = GetCatById(catId);
        cat.UnassignAdvertisement();
    }

    private void UpdateAdvertisementPriorityScore(Guid advertisementId)
    {
        IEnumerable<Cat> catsQuery = GetAssignedToConcreteAdvertisementCats(advertisementId);
        double catsToAssignToAdvertisementHighestPriorityScore =
            GetHighestPriorityScoreFromGivenCats(catsQuery.Select(x => x.Id));

        Advertisement advertisement = GetAdvertisementById(advertisementId);
        advertisement.PriorityScore = catsToAssignToAdvertisementHighestPriorityScore;
    }

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

    private static void ThrowIfCatIsAssignedToAdvertisement(Cat cat)
    {
        if (cat.AdvertisementId.HasValue)
        {
            throw new InvalidOperationException(string.Format(ErrorMessages.CatIsAssignedToAdvertisement, cat.Id,
                cat.AdvertisementId));
        }
    }

    private Cat GetCatById(Guid catId) =>
        _cats.FirstOrDefault(c => c.Id == catId) ??
        throw new NotFoundExceptions.CatNotFoundException(catId);

    private Advertisement GetAdvertisementById(Guid advertisementId) =>
        _advertisements.FirstOrDefault(c => c.Id == advertisementId) ??
        throw new NotFoundExceptions.AdvertisementNotFoundException(advertisementId);

    // 11. Private constants/error messages
    private static class ErrorMessages
    {
        public const string CatIsAssignedToAdvertisement =
            "Cat with id: '{0}' is assigned to advertisement with id: {1}, so it can not be removed.";

        public const string InvalidCatsOwnership = "One or more provided cats do not belong to provided person.";
        public const string EmptyCatsList = "Advertisement cats list must not be empty.";
    }
}

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
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

        builder.HasMany(person => person.Advertisements)
            .WithOne()
            .HasForeignKey(advertisement => advertisement.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ComplexProperty(x => x.DefaultAdvertisementsPickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Country)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.Country)}")
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.State)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.State)}")
                .HasMaxLength(Address.StateMaxLength);

            complexPropertyBuilder.Property(x => x.ZipCode)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.ZipCode)}")
                .HasMaxLength(Address.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.City)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.City)}")
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.Street)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.Street)}")
                .HasMaxLength(Address.StreetMaxLength);

            complexPropertyBuilder.Property(x => x.BuildingNumber)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.BuildingNumber)}")
                .HasMaxLength(Address.BuildingNumberMaxLength);
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfoEmail, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.DefaultAdvertisementsContactInfoEmail)}")
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfoPhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.DefaultAdvertisementsContactInfoPhoneNumber)}")
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Nickname, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.Nickname)}")
                .HasMaxLength(Nickname.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Email, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.Email)}")
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.PhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.PhoneNumber)}")
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.Property(x => x.UserIdentityId)
            .IsRequired();
    }
}