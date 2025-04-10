using KittySaver.ReadModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.ReadModels.EntityFramework.PersonAggregate;

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