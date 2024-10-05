using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Cat : AuditableEntity
{
    public required string Name { get; set; }
    public string? AdditionalRequirements { get; set; }
    public required bool IsCastrated { get; set; }
    public required bool IsInNeedOfSeeingVet { get; set; }
    public required MedicalHelpUrgency MedicalHelpUrgency { get; set; }
    public required AgeCategory AgeCategory { get; set; }
    public required Behavior Behavior { get; set; }
    public required HealthStatus HealthStatus { get; set; }
    
    public required Guid PersonId { get; init; }
    public Person Person { get; private set; } = null!;
}

internal class CatConfiguration : IEntityTypeConfiguration<Cat>
{
    public void Configure(EntityTypeBuilder<Cat> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.AdditionalRequirements)
            .HasMaxLength(1000);
        builder.Property(x => x.IsCastrated).IsRequired();
        builder.Property(x => x.IsInNeedOfSeeingVet).IsRequired();
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.AgeCategory).IsRequired();
        builder.Property(x => x.Behavior).IsRequired();
        builder.Property(x => x.HealthStatus).IsRequired();
    }
}