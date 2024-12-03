using System.Diagnostics.CodeAnalysis;
using KittySaver.Domain.Advertisements.Events;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Domain.Advertisements;

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
        List<Guid> catsIdsToAssignList = catsIdsToAssign.ToList();
        if (catsIdsToAssignList.Count == 0)
        {
            throw new ArgumentException(ErrorMessages.EmptyCatsList, nameof(catsIdsToAssign));
        }
        
        DateTimeOffset expiresOn = currentDate + ExpiringPeriodInDays;
        
        Advertisement advertisement = new(
            personId: person.Id,
            pickupAddress: pickupAddress,
            contactInfoEmail: contactInfoEmail,
            contactInfoPhoneNumber: contactInfoPhoneNumber,
            description: description,
            expiresOn: expiresOn);
        
        foreach (Guid catId in catsIdsToAssignList)
        {
            person.AssignCatToAdvertisement(advertisement.Id, catId);
        }
        double catsToAssignToAdvertisementHighestPriorityScore = person.GetHighestPriorityScoreFromGivenCats(catsIdsToAssignList);
        advertisement.PriorityScore = catsToAssignToAdvertisementHighestPriorityScore;
        
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

    public static readonly TimeSpan ExpiringPeriodInDays = new(days: 30, hours: 0, minutes: 0, seconds: 0);
    private readonly Guid _personId;
    private double _priorityScore;
    public enum AdvertisementStatus
    {
        Draft,
        Active,
        Closed,
        Expired //TODO: Background running task every day
    }
    public AdvertisementStatus Status { get; private set; } = AdvertisementStatus.Draft;
    public DateTimeOffset? ClosedOn { get; private set; }
    public DateTimeOffset ExpiresOn { get; private set; }

    public double PriorityScore
    {
        get => _priorityScore;
        internal set => _priorityScore = value == 0 
            ? throw new ArgumentException(ErrorMessages.ZeroPriorityScore, nameof(PriorityScore))
            : value;
    }

    public required Guid PersonId
    {
        get => _personId;
        init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException(ErrorMessages.EmptyPersonId, nameof(PersonId));
            }
            _personId = value;
        }
    }
    public Description Description { get; private set; }
    public Address PickupAddress { get; private set; }
    public Email ContactInfoEmail { get; private set; }
    public PhoneNumber ContactInfoPhoneNumber { get; private set; }

    public void ChangeDescription(Description description)
    {
        Description = description;
    }
    
    public void ChangePickupAddress(Address pickupAddress)
    {
        PickupAddress = pickupAddress;
    }

    public void ChangeContactInfo(Email contactInfoEmail, PhoneNumber contactInfoPhoneNumber)
    {
        ContactInfoEmail = contactInfoEmail;
        ContactInfoPhoneNumber = contactInfoPhoneNumber;
    }

    public void Close(DateTimeOffset currentDate)
    {
        EnsureAdvertisementIsActive();

        ClosedOn = currentDate;
        Status = AdvertisementStatus.Closed;
        
        RaiseDomainEvent(new AdvertisementClosedDomainEvent(Id));
    }

    public void Expire(DateTimeOffset currentDate)
    {
        EnsureAdvertisementIsActive();

        if (ExpiresOn <= currentDate)
        {
            Status = AdvertisementStatus.Expired;
        }
    }

    public void Refresh(DateTimeOffset currentDate)
    {
        if (Status is not (AdvertisementStatus.Active or AdvertisementStatus.Expired))
        {
            throw new InvalidOperationException(ErrorMessages.InvalidRefreshOperation);
        }

        ExpiresOn = currentDate + ExpiringPeriodInDays;
        if (Status is not AdvertisementStatus.Active)
        {
            Status = AdvertisementStatus.Active;
        }
    }
    
    public void AnnounceDeletion() => RaiseDomainEvent(new AdvertisementDeletedDomainEvent(Id, PersonId));

    public void ValidateOwnership(Guid personId)
    {
        if (personId != PersonId)
        {
            throw new ArgumentException(ErrorMessages.InvalidPersonId, nameof(personId));
        }
    }
    
    private void EnsureAdvertisementIsActive()
    {
        if (Status is not AdvertisementStatus.Active)
        {
            throw new InvalidOperationException(ErrorMessages.InvalidStatusOperation);
        }
    }

    private static class ErrorMessages
    {
        public const string EmptyCatsList = "Advertisement cats list must not be empty.";
        public const string ZeroPriorityScore = "PriorityScore cannot be zero, probably something went wrong.";
        public const string EmptyPersonId = "Provided person id is empty.";
        public const string InvalidPersonId = "Provided person id is not id of advertisement owner.";
        public const string InvalidStatusOperation = "Active advertisement status is required for that operation.";
        public const string InvalidRefreshOperation = "You cannot refresh an advertisement that is not active/expired.";
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
                .HasColumnName($"{nameof(Advertisement.Description)}")
                .HasMaxLength(Description.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.ContactInfoEmail, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Advertisement.ContactInfoEmail)}")
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });
        
        builder.ComplexProperty(x => x.ContactInfoPhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Advertisement.ContactInfoPhoneNumber)}")
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.PickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Country)
                .HasColumnName($"{nameof(Advertisement.PickupAddress)}{nameof(Advertisement.PickupAddress.Country)}")
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.State)
                .HasColumnName($"{nameof(Advertisement.PickupAddress)}{nameof(Advertisement.PickupAddress.State)}")
                .HasMaxLength(Address.StateMaxLength);

            complexPropertyBuilder.Property(x => x.ZipCode)
                .HasColumnName($"{nameof(Advertisement.PickupAddress)}{nameof(Advertisement.PickupAddress.ZipCode)}")
                .HasMaxLength(Address.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.City)
                .HasColumnName($"{nameof(Advertisement.PickupAddress)}{nameof(Advertisement.PickupAddress.City)}")
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.Street)
                .HasColumnName($"{nameof(Advertisement.PickupAddress)}{nameof(Advertisement.PickupAddress.Street)}")
                .HasMaxLength(Address.StreetMaxLength);

            complexPropertyBuilder.Property(x => x.BuildingNumber)
                .HasColumnName($"{nameof(Advertisement.PickupAddress)}{nameof(Advertisement.PickupAddress.BuildingNumber)}")
                .HasMaxLength(Address.BuildingNumberMaxLength);
        });
    }
}