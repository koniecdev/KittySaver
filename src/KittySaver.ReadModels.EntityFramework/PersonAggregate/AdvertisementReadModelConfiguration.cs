using KittySaver.ReadModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.ReadModels.EntityFramework.PersonAggregate;

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