using System.Diagnostics.CodeAnalysis;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Domain.Persons;

public sealed class Advertisement : AuditableEntity
{
    public static readonly TimeSpan ExpiringPeriodInDays = new(days: 30, hours: 0, minutes: 0, seconds: 0);
    public static readonly TimeSpan ShelterExpiringPeriodInDays = new(days: 365, hours: 0, minutes: 0, seconds: 0);

    private readonly Guid _personId;
    private double _priorityScore;

    public enum AdvertisementStatus
    {
        Active,
        Closed,
        Expired,
        ThumbnailNotUploaded
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

    public double PriorityScore
    {
        get => _priorityScore;
        internal set => _priorityScore = value == 0 
            ? throw new ArgumentException(ErrorMessages.ZeroPriorityScore, nameof(PriorityScore))
            : value;
    }

    public AdvertisementStatus Status { get; private set; } = AdvertisementStatus.ThumbnailNotUploaded;
    public DateTimeOffset? ClosedOn { get; private set; }
    public DateTimeOffset ExpiresOn { get; private set; }
    public Description Description { get; private set; }
    public Address PickupAddress { get; private set; }
    public Email ContactInfoEmail { get; private set; }
    public PhoneNumber ContactInfoPhoneNumber { get; private set; }

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
        DateTimeOffset expiresOn,
        double priorityScore)
    {
        PersonId = personId;
        PickupAddress = pickupAddress;
        ContactInfoEmail = contactInfoEmail;
        ContactInfoPhoneNumber = contactInfoPhoneNumber;
        Description = description;
        ExpiresOn = expiresOn;
        PriorityScore = priorityScore;
    }

    internal static Advertisement Create(
        DateTimeOffset dateOfCreation,
        Guid ownerId,
        Person.Role ownerRole,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber,
        Description description,
        double priorityScore)
    {
        DateTimeOffset expiresOn = dateOfCreation + (ownerRole == Person.Role.Shelter ? ShelterExpiringPeriodInDays : ExpiringPeriodInDays);
        Advertisement advertisement = new(
            personId: ownerId,
            pickupAddress: pickupAddress,
            contactInfoEmail: contactInfoEmail,
            contactInfoPhoneNumber: contactInfoPhoneNumber,
            description: description,
            expiresOn: expiresOn,
            priorityScore: priorityScore);
        return advertisement;
    }

    internal void ChangeDescription(Description description)
    {
        Description = description;
    }
    
    internal void ChangePickupAddress(Address pickupAddress)
    {
        PickupAddress = pickupAddress;
    }

    internal void ChangeContactInfo(Email contactInfoEmail, PhoneNumber contactInfoPhoneNumber)
    {
        ContactInfoEmail = contactInfoEmail;
        ContactInfoPhoneNumber = contactInfoPhoneNumber;
    }

    internal void Activate()
    {
        if (Status is not AdvertisementStatus.ThumbnailNotUploaded)
        {
            throw new InvalidOperationException(ErrorMessages.ThumbnailNotUploadedStatusIsRequiredOperation);
        }
        Status = AdvertisementStatus.Active;
    }
    
    internal void Close(DateTimeOffset currentDate)
    {
        EnsureAdvertisementIsActive();

        ClosedOn = currentDate;
        Status = AdvertisementStatus.Closed;
    }

    internal void Expire(DateTimeOffset currentDate)
    {
        EnsureAdvertisementIsActive();

        if (ExpiresOn <= currentDate)
        {
            Status = AdvertisementStatus.Expired;
        }
    }

    internal void Refresh(DateTimeOffset currentDate)
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

    private void EnsureAdvertisementIsActive()
    {
        if (Status is not AdvertisementStatus.Active)
        {
            throw new InvalidOperationException(ErrorMessages.ActiveStatusIsRequiredOperation);
        }
    }

    private static class ErrorMessages
    {
        public const string ZeroPriorityScore = "PriorityScore cannot be zero, probably something went wrong.";
        public const string EmptyPersonId = "Provided person id is empty.";
        public const string ActiveStatusIsRequiredOperation = "Active advertisement status is required for that operation.";
        public const string ThumbnailNotUploadedStatusIsRequiredOperation = "Thumbnail not uploaded advertisement status is required for that operation.";
        public const string InvalidRefreshOperation = "You cannot refresh an advertisement that is not active/expired.";
    }
}

public sealed class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
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