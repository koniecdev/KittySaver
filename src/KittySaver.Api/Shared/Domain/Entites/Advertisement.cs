using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Advertisement : AuditableEntity
{
    public static Advertisement Create(
        DateTimeOffset currentDate,
        Person person,
        IEnumerable<Cat> cats,
        PickupAddress pickupAddress,
        ContactInfo contactInfo,
        string? description = null)
    {
        List<Cat> catsToAssign = cats.ToList();
        ValidateCats(person, catsToAssign);

        DateTimeOffset expiresOn = currentDate.AddDays(30);

        double catsHighestPriorityScore = catsToAssign.Max(cat => cat.PriorityScore);

        Advertisement advertisement = new(
            person: person,
            cats: catsToAssign,
            pickupAddress: pickupAddress,
            contactInfo: contactInfo,
            description: description,
            expiresOn: expiresOn,
            priorityScore: catsHighestPriorityScore);

        foreach (Cat cat in advertisement.Cats)
        {
            cat.AssignAdvertisement(advertisement);
        }
        
        person.AddAdvertisement(advertisement);

        return advertisement;
        
        static void ValidateCats(Person person, IEnumerable<Cat> cats)
        {
            List<Cat> catsToValidate = cats.ToList();
            if (catsToValidate.Count == 0)
            {
                throw new ArgumentException("Advertisement cats list must not be empty.", nameof(cats));
            }

            if (!catsToValidate.All(cat => person.Cats.Contains(cat)))
            {
                throw new ArgumentException("One or more provided cats do not belong to provided person.",
                    nameof(cats));
            }

            if (!catsToValidate.Any(cat => cat.AdvertisementId is not null || cat.Advertisement is not null))
            {
                return;
            }

            List<Cat> catsThatAreAlreadyAssigned = catsToValidate
                .Where(cat => cat.AdvertisementId is not null && cat.Advertisement is not null)
                .ToList();

            throw new InvalidOperationException(
                $"Cats: {string.Join(",", catsThatAreAlreadyAssigned.Select(x => x.Id))} are already assigned to another advertisement");
        }
    }

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Advertisement()
    {
        Person = null!;
    }

    [SetsRequiredMembers]
    private Advertisement(
        Person person,
        List<Cat> cats,
        PickupAddress pickupAddress,
        ContactInfo contactInfo,
        string? description,
        DateTimeOffset expiresOn,
        double priorityScore)
    {
        _cats = cats;
        PersonId = person.Id;
        Person = person;
        PickupAddress = pickupAddress;
        ContactInfo = contactInfo;
        Description = description;
        ExpiresOn = expiresOn;
        PriorityScore = priorityScore;
    }

    private readonly Guid _personId;

    private List<Cat> _cats = [];

    public DateTimeOffset? ClosedOn { get; private set; }

    public DateTimeOffset ExpiresOn { get; private set; }

    public AdvertisementStatus Status { get; private set; } = AdvertisementStatus.Active;

    public double PriorityScore { get; private set; }

    public string? Description { get; set; }

    public required Guid PersonId
    {
        get => _personId;
        init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Provided person id is empty", nameof(PersonId));
            }

            _personId = value;
        }
    }

    public Person Person { get; private init; }

    public IReadOnlyList<Cat> Cats => _cats.ToList();

    public required PickupAddress PickupAddress { get; set; }

    public required ContactInfo ContactInfo { get; set; }

    public void Close(DateTimeOffset currentDate)
    {
        if (Status is not AdvertisementStatus.Active)
        {
            throw new InvalidOperationException("You can not close advertisement that is not active");
        }

        foreach (Cat cat in _cats)
        {
            cat.MarkAsAdopted();
        }

        ClosedOn = currentDate;
        Status = AdvertisementStatus.Closed;
    }

    public void Expire(DateTimeOffset currentDate)
    {
        if (Status is not AdvertisementStatus.Active)
        {
            throw new InvalidOperationException("You can not expire advertisement that is not active");
        }

        if (ExpiresOn <= currentDate)
        {
            Status = AdvertisementStatus.Expired;
        }
    }

    public void Refresh(DateTimeOffset currentDate)
    {
        if (Status is not (AdvertisementStatus.Active or AdvertisementStatus.Expired))
        {
            throw new InvalidOperationException("You can not refresh advertisement that is not active/expired");
        }

        ExpiresOn = currentDate.AddDays(30);
    }

    public void AddCat(Cat cat)
    {
        if (PersonId != cat.PersonId)
        {
            throw new InvalidOperationException("Provided cat that do not belong to creator of advertisement.");
        }

        cat.AssignAdvertisement(this);
        ReCalculatePriorityScore();
    }

    public void RemoveCat(Cat cat)
    {
        if (PersonId != cat.PersonId)
        {
            throw new InvalidOperationException("Provided cat that do not belong to creator of advertisement.");
        }

        if (cat.AdvertisementId != Id)
        {
            throw new InvalidOperationException("Provided cat have none, or complety different advertisement assigned.");
        }
        
        cat.UnassignAdvertisement();
        ReCalculatePriorityScore();
    }
    
    private void ReCalculatePriorityScore()
    {
        double catsMaximumPriorityScore = _cats.Max(x => x.PriorityScore);
        PriorityScore = catsMaximumPriorityScore;
    }

    public static class Constraints
    {
        public const int DescriptionMaxLength = 2000;
    }

    public enum AdvertisementStatus
    {
        Active,
        Closed,
        Expired //TODO: Background running task every day
    }
}

internal sealed class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("Advertisements");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PersonId).IsRequired();

        builder.Property(x => x.Description).HasMaxLength(Advertisement.Constraints.DescriptionMaxLength);

        builder.ComplexProperty(x => x.PickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Country)
                .HasMaxLength(IAddress.Constraints.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.State)
                .HasMaxLength(IAddress.Constraints.StateMaxLength);

            complexPropertyBuilder.Property(x => x.ZipCode)
                .HasMaxLength(IAddress.Constraints.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.City)
                .HasMaxLength(IAddress.Constraints.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.Street)
                .HasMaxLength(IAddress.Constraints.StreetMaxLength);

            complexPropertyBuilder.Property(x => x.BuildingNumber)
                .HasMaxLength(IAddress.Constraints.BuildingNumberMaxLength);
        });

        builder.ComplexProperty(x => x.ContactInfo, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Email)
                .HasMaxLength(IContact.Constraints.EmailMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.PhoneNumber)
                .HasMaxLength(IContact.Constraints.PhoneNumberMaxLength);
        });
    }
}