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
        IEnumerable<Guid> catsIds,
        PickupAddress pickupAddress,
        ContactInfo contactInfo,
        string? description = null)
    {
        DateTimeOffset expiresOn = currentDate + ExpiringPeriodInDays;
        
        List<Guid> catsIdsToAssignToAdvertisements = catsIds.ToList();
        if (catsIdsToAssignToAdvertisements.Count == 0)
        {
            throw new ArgumentException("Advertisement cats list must not be empty.", nameof(catsIds));
        }
        
        double catsHighestPriorityScore = person.GetHighestPriorityScoreFromGivenCats(catsIdsToAssignToAdvertisements);
        
        Advertisement advertisement = new(
            personId: person.Id,
            pickupAddress: pickupAddress,
            contactInfo: contactInfo,
            description: description,
            expiresOn: expiresOn,
            priorityScore: catsHighestPriorityScore);
        person.AssignGivenCatsToAdvertisement(advertisement.Id, catsIdsToAssignToAdvertisements);
        return advertisement;
    }

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Advertisement()
    {
    }

    [SetsRequiredMembers]
    private Advertisement(
        Guid personId,
        PickupAddress pickupAddress,
        ContactInfo contactInfo,
        string? description,
        DateTimeOffset expiresOn,
        double priorityScore)
    {
        PersonId = personId;
        PickupAddress = pickupAddress;
        ContactInfo = contactInfo;
        Description = description;
        ExpiresOn = expiresOn;
        PriorityScore = priorityScore;
    }

    private static readonly TimeSpan ExpiringPeriodInDays = new(days: 30, hours: 0, minutes: 0, seconds: 0);
    private readonly Guid _personId;
    private double _priorityScore;

    public DateTimeOffset? ClosedOn { get; private set; }

    public DateTimeOffset ExpiresOn { get; private set; }

    public AdvertisementStatus Status { get; private set; } = AdvertisementStatus.Active;

    public double PriorityScore
    {
        get => _priorityScore;
        private set
        {
            if (value == 0)
            {
                throw new Exception(); //yikes - specific exception
            }
            _priorityScore = value;
        }
    }

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
    
    public required PickupAddress PickupAddress { get; set; }

    public required ContactInfo ContactInfo { get; set; }

    public void Close(DateTimeOffset currentDate)
    {
        if (Status is not AdvertisementStatus.Active)
        {
            throw new InvalidOperationException("You can not close advertisement that is not active");
        }

        //TODO: Marking cats as adopted should be handled by Domain Event
        // foreach (Cat cat in _cats)
        // {
        //     cat.MarkAsAdopted();
        // }

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

        ExpiresOn = currentDate + ExpiringPeriodInDays;
    }

    private void ReCalculatePriorityScore()//this do not belongs here, probably DomainService
    {
        // double catsMaximumPriorityScore = _cats.Max(x => x.PriorityScore);
        // PriorityScore = catsMaximumPriorityScore;
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
        
        builder.HasMany<Cat>()
            .WithOne()
            .HasForeignKey(cat => cat.AdvertisementId)
            .IsRequired(false);

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