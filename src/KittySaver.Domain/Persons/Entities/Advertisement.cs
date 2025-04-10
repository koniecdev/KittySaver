using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Domain.Persons.Entities;

public sealed class Advertisement : AuditableEntity<AdvertisementId>
{
    public static readonly TimeSpan ExpiringPeriodInDays = TimeSpan.FromDays(30);
    public static readonly TimeSpan ShelterExpiringPeriodInDays = TimeSpan.FromDays(365);

    private readonly PersonId _personId;
    private double _priorityScore;

    public PersonId PersonId
    {
        get => _personId;
        private init
        {
            if (value == PersonId.Empty)
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
    private Advertisement() : base(default)
    {
        Description = null!;
        PickupAddress = null!;
        ContactInfoEmail = null!;
        ContactInfoPhoneNumber = null!;
    }

    private Advertisement(
        AdvertisementId id,
        PersonId personId,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber,
        Description description,
        DateTimeOffset expiresOn,
        double priorityScore) : base(id)
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
        PersonId ownerId,
        PersonRole ownerRole,
        Address pickupAddress,
        Email contactInfoEmail,
        PhoneNumber contactInfoPhoneNumber,
        Description description,
        double priorityScore)
    {
        AdvertisementId id = AdvertisementId.New();
        DateTimeOffset expiresOn = dateOfCreation + (ownerRole == PersonRole.Shelter ? ShelterExpiringPeriodInDays : ExpiringPeriodInDays);
        Advertisement advertisement = new(
            id,
            ownerId,
            pickupAddress,
            contactInfoEmail,
            contactInfoPhoneNumber,
            description,
            expiresOn,
            priorityScore);
        return advertisement;
    }

    internal void SetDescription(Description description)
    {
        Description = description;
    }
    
    internal void SetPickupAddress(Address pickupAddress)
    {
        PickupAddress = pickupAddress;
    }

    internal void UpdateContactInfo(Email contactInfoEmail, PhoneNumber contactInfoPhoneNumber)
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