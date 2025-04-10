using KittySaver.ReadModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.ReadModels.EntityFramework.PersonAggregate;

internal sealed class CatReadModelConfiguration : IEntityTypeConfiguration<CatReadModel>, IReadConfiguration
{
    public void Configure(EntityTypeBuilder<CatReadModel> builder)
    {
        builder.ToTable("Cats");
        
        builder.HasKey(cat => cat.Id);
    }
}