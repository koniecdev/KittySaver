using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed class AdvertisementReadModel
{
    public required Guid Id { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTimeOffset CreatedOn { get; init; }
    public required string? LastModificationBy { get; init; }
    public required DateTimeOffset? LastModificationOn { get; init; }
    public required DateTimeOffset? ClosedOn { get; init; }
    public required DateTimeOffset ExpiresOn { get; init; }
    public required Advertisement.AdvertisementStatus Status { get; init; }
    public required double PriorityScore { get; init; }
    public required string ContactInfoEmail { get; init; }
    public required string ContactInfoPhoneNumber { get; init; }
    public required string Description { get; init; }
    public required string? PickupAddressBuildingNumber { get; init; }
    public required string PickupAddressCity { get; init; }
    public required string PickupAddressCountry { get; init; }
    public required string? PickupAddressState { get; init; }
    public required string? PickupAddressStreet { get; init; }
    public required string PickupAddressZipCode { get; init; }
    public required Guid PersonId { get; init; }
    public PersonReadModel Person { get; private set; } = null!;
    public ICollection<CatReadModel> Cats { get; } = new List<CatReadModel>();
}

internal sealed class AdvertisementReadModelConfiguration : IEntityTypeConfiguration<AdvertisementReadModel>, IReadConfiguration
{
    public void Configure(EntityTypeBuilder<AdvertisementReadModel> builder)
    {
        builder.ToTable("Advertisements");

        builder.HasKey(advertisement => advertisement.Id);

        builder.HasMany(advertisement => advertisement.Cats)
            .WithOne(cat => cat.Advertisement)
            .HasForeignKey(cat => cat.AdvertisementId)
            .IsRequired(false);
    }
}