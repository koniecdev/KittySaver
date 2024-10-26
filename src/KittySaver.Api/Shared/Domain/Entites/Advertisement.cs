using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Advertisement : AuditableEntity
{
    private readonly Guid _personId;
    
    private List<Cat> _cats = [];
    
    public DateTimeOffset? ActivatedOn { get; private set; }
    
    public DateTimeOffset? ExpiresOn { get; private set; }
    
    public AdvertisementStatus Status { get; private set; } = AdvertisementStatus.Draft;
    
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
    
    public required PickupAddress PickupAddress { get; set; }
    
    public required ContactInfo ContactInfo { get; set; }
    
    public static class Constraints
    {
        public const int DescriptionMaxLength = 2000;
    }
    
    public enum AdvertisementStatus
    {
        Draft,
        Active,
        Paused,
        Closed,
        Expired
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