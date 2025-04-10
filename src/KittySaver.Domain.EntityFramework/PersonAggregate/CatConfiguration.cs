using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Domain.Persons.Entities;

public sealed class CatConfiguration : IEntityTypeConfiguration<Cat>
{
    public void Configure(EntityTypeBuilder<Cat> builder)
    {
        builder.ToTable("Cats");
        
        builder.Property(x => x.Id).ValueGeneratedNever();
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PersonId).IsRequired();

        builder.ComplexProperty(x => x.Name, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Cat.Name)}")
                .HasMaxLength(CatName.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.AdditionalRequirements, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Cat.AdditionalRequirements)}")
                .HasMaxLength(Description.MaxLength)
                .IsRequired();
        });

        builder.Property(x => x.AgeCategory)
            .HasConversion(
                v => v.Name,
                v => AgeCategory.FromNameOrValue(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Behavior)
            .HasConversion(
                v => v.Name,
                v => Behavior.FromNameOrValue(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.HealthStatus)
            .HasConversion(
                v => v.Name,
                v => HealthStatus.FromNameOrValue(v, true))
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.MedicalHelpUrgency)
            .HasConversion(
                v => v.Name,
                v => MedicalHelpUrgency.FromNameOrValue(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PriorityScore)
            .IsRequired(); 
    }
}