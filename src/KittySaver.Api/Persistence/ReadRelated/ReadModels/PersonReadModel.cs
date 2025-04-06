using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed class PersonReadModel
{
    public required PersonId Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required int CurrentRole { get; init; }
    public required Guid UserIdentityId { get; init; }
    public required string DefaultAdvertisementsContactInfoEmail { get; init; }
    public required string DefaultAdvertisementsContactInfoPhoneNumber { get; init; }
    public required string DefaultAdvertisementsPickupAddressBuildingNumber { get; init; }
    public required string DefaultAdvertisementsPickupAddressCity { get; init; }
    public required string DefaultAdvertisementsPickupAddressCountry { get; init; }
    public required string? DefaultAdvertisementsPickupAddressState { get; init; }
    public required string DefaultAdvertisementsPickupAddressStreet { get; init; }
    public required string DefaultAdvertisementsPickupAddressZipCode { get; init; }
    public required string Email { get; init; }
    public required string Nickname { get; init; }
    public required string PhoneNumber { get; init; }
    public ICollection<CatReadModel> Cats { get; } = new List<CatReadModel>();
    public ICollection<AdvertisementReadModel> Advertisements { get; } = new List<AdvertisementReadModel>();
}

internal sealed class PersonReadModelConfiguration : IEntityTypeConfiguration<PersonReadModel>, IReadConfiguration
{
    public void Configure(EntityTypeBuilder<PersonReadModel> builder)
    {
        builder.ToTable("Persons");
        
        builder.HasKey(person => person.Id);

        builder
            .HasMany(person => person.Cats)
            .WithOne(cat => cat.Person)
            .HasForeignKey(cat => cat.PersonId)
            .IsRequired();
        
        builder
            .HasMany(person => person.Advertisements)
            .WithOne(advertisement => advertisement.Person)
            .HasForeignKey(advertisement => advertisement.PersonId)
            .IsRequired();
    }
}