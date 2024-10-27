using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Advertisement : AuditableEntity
{
    public Advertisement Create(
        DateTimeOffset currentDate,
        Person person,
        IEnumerable<Cat> cats,
        PickupAddress pickupAddress,
        ContactInfo contactInfo,
        string? description = null)
    {
        List<Cat> catsToAssign = SetCats(person, cats);

        DateTimeOffset expiresOn = currentDate.AddDays(30);

        double catsMaximumPriorityScore = catsToAssign.Max(cat => cat.PriorityScore);
        
        Advertisement advertisement = new(
            personId: person.Id,
            cats: catsToAssign,
            pickupAddress: pickupAddress,
            contactInfo: contactInfo,
            description: description,
            expiresOn: expiresOn,
            priorityScore: catsMaximumPriorityScore);
        
        return advertisement;
    }

    [SetsRequiredMembers]
    private Advertisement(
        Guid personId,
        IEnumerable<Cat> cats,
        PickupAddress pickupAddress,
        ContactInfo contactInfo,
        string? description,
        DateTimeOffset expiresOn,
        double priorityScore)
    {
        _cats = cats.ToList();
        PersonId = personId;
        PickupAddress = pickupAddress;
        ContactInfo = contactInfo;
        Description = description;
        ExpiresOn = expiresOn;
    }
    
    private readonly Guid _personId;
    
    private List<Cat> _cats;
    
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
    
    public Person Person { get; private set; } = null!;

    public IReadOnlyList<Cat> Cats => _cats.ToList();
    private static List<Cat> SetCats(Person person, IEnumerable<Cat> cats)
    {
        List<Cat> catsToAssign = cats.ToList();
        if (catsToAssign.Count == 0)
        {
            throw new ArgumentException("Advertisement cats list must not be empty.", nameof(cats));
        }
        
        if (!catsToAssign.All(cat => person.Cats.Contains(cat)))
        {
            throw new ArgumentException("All provided cats for advertisement must be owned by single person.", nameof(cats));
        }

        if (!catsToAssign.Any(cat => cat.AdvertisementId is not null))
        {
            return catsToAssign;
        }
        
        List<Cat> catsThatAreAlreadyAssigned = catsToAssign
            .Where(cat => cat.AdvertisementId is not null)
            .ToList();
        
        throw new ArgumentException(
            $"Cats: {string.Join(",", catsThatAreAlreadyAssigned.Select(x=>x.Id))} are already assigned to another advertisement",
            nameof(cats));
    }
    
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

    public void ReCalculatePriorityScore()
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
        builder.Property(x => x.PersonId).IsRequired();
        builder.ComplexProperty(x => x.ContactInfo).IsRequired();
        builder.ComplexProperty(x => x.PickupAddress).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(Advertisement.Constraints.DescriptionMaxLength);
    }
}