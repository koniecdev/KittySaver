using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Shared.Domain.Advertisement.Events;
using KittySaver.Api.Shared.Domain.Common.Primitives;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Advertisement;

public sealed class Advertisement : AggregateRoot
{
    public static Advertisement Create(
        DateTimeOffset currentDate,
        Person person,
        IEnumerable<Guid> catsIdsToAssign,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber,
        Description description)
    {
        DateTimeOffset expiresOn = currentDate + ExpiringPeriodInDays;
        
        List<Guid> catsIdsToAssignToAdvertisement = catsIdsToAssign.ToList();
        if (catsIdsToAssignToAdvertisement.Count == 0)
        {
            throw new ArgumentException("Advertisement cats list must not be empty.", nameof(catsIdsToAssign));
        }
        
        Advertisement advertisement = new(
            personId: person.Id,
            pickupAddress: pickupAddress,
            contactInfoEmail: contactInfoEmail,
            contactInfoPhoneNumber: contactInfoPhoneNumber,
            description: description,
            expiresOn: expiresOn);
        
        foreach (Guid catId in catsIdsToAssignToAdvertisement)
        {
            person.AssignCatToDraftAdvertisement(advertisement.Id, catId);
        }
        
        AdvertisementService advertisementService = AdvertisementService.Create(advertisement, person);
        advertisementService.RecalculatePriorityScore();

        advertisement.Status = AdvertisementStatus.Active;
        return advertisement;
    }

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Advertisement()
    {
        Description = null!;
        PickupAddress = null!;
        ContactInfoEmail = null!;
        ContactInfoPhoneNumber = null!;
    }

    [SetsRequiredMembers]
    private Advertisement(
        Guid personId,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber,
        Description description,
        DateTimeOffset expiresOn)
    {
        PersonId = personId;
        PickupAddress = pickupAddress;
        ContactInfoEmail = contactInfoEmail;
        ContactInfoPhoneNumber = contactInfoPhoneNumber;
        Description = description;
        ExpiresOn = expiresOn;
    }

    private static readonly TimeSpan ExpiringPeriodInDays = new(days: 30, hours: 0, minutes: 0, seconds: 0);
    private readonly Guid _personId;
    private double _priorityScore;

    public DateTimeOffset? ClosedOn { get; private set; }

    public DateTimeOffset ExpiresOn { get; private set; }

    public AdvertisementStatus Status { get; private set; } = AdvertisementStatus.Draft;

    public double PriorityScore
    {
        get => _priorityScore;
        internal set
        {
            if (value == 0)
            {
                throw new ArgumentException("PriorityScore can not be zero, probably something went wrong.", nameof(PriorityScore));
            }
            _priorityScore = value;
        }
    }

    public Description Description { get; set; }

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
    
    public required Address PickupAddress { get; set; }
    public required Email ContactInfoEmail { get; set; }
    public required PhoneNumber ContactInfoPhoneNumber { get; set; }

    public void Close(DateTimeOffset currentDate)
    {
        if (Status is not AdvertisementStatus.Active)
        {
            throw new InvalidOperationException("You can not close advertisement that is not active");
        }

        ClosedOn = currentDate;
        Status = AdvertisementStatus.Closed;
        
        RaiseDomainEvent(new AdvertisementClosedDomainEvent(Id));
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

    public enum AdvertisementStatus
    {
        Draft,
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
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.Property(x => x.PersonId).IsRequired();
        
        builder.ComplexProperty(x => x.Description, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(Description.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.ContactInfoEmail, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });
        
        builder.ComplexProperty(x => x.ContactInfoPhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.PickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Country)
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.State)
                .HasMaxLength(Address.StateMaxLength);

            complexPropertyBuilder.Property(x => x.ZipCode)
                .HasMaxLength(Address.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.City)
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.Street)
                .HasMaxLength(Address.StreetMaxLength);

            complexPropertyBuilder.Property(x => x.BuildingNumber)
                .HasMaxLength(Address.BuildingNumberMaxLength);
        });
    }
}