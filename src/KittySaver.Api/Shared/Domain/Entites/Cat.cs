using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Shared.Exceptions;
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
    
    public const int NameMaxLength = 100;
    public const int AdditionalRequirementsMaxLength = 1000;

    public double GetPriorityScore(int daysAwaiting, ICatPriorityCalculator calculator)
    {
        double priority = calculator.Calculate(this, daysAwaiting);
        return priority;
    }
}

internal sealed class CatConfiguration : IEntityTypeConfiguration<Cat>
{
    public void Configure(EntityTypeBuilder<Cat> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(Cat.NameMaxLength)
            .IsRequired();
        builder.Property(x => x.AdditionalRequirements)
            .HasMaxLength(Cat.AdditionalRequirementsMaxLength);
        builder.Property(x => x.IsCastrated).IsRequired();
        builder.Property(x => x.IsInNeedOfSeeingVet).IsRequired();
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.AgeCategory).IsRequired();
        builder.Property(x => x.Behavior).IsRequired();
        builder.Property(x => x.HealthStatus).IsRequired();
    }
}

public interface ICatPriorityCalculator
{
    public double Calculate(Cat cat, int daysAwaiting);
}

internal class DefaultCatPriorityCalculator : ICatPriorityCalculator
{
    public double Calculate(Cat cat, int daysAwaiting)
    {
        const double healthWeight = 2.0;
        const double medicalHelpUrgencyWeight = 2.0;
        const double behaviorWeight = 1.5;
        const double ageWeight = 1.2;
        const double daysAwaitingWeight = 0.5;
    
        int healthStatusPoints = cat.HealthStatus.MaxScorePoints - cat.HealthStatus.ScorePoints;
        int behaviourPoints = cat.Behavior.MaxScorePoints - cat.Behavior.ScorePoints;
        int medicalHelpUrgencyPoints =  cat.MedicalHelpUrgency.MaxScorePoints - cat.MedicalHelpUrgency.ScorePoints;
        int ageCategoryPoints = cat.AgeCategory.MaxScorePoints - cat.AgeCategory.ScorePoints;
        
        double priority = 
            healthStatusPoints * healthWeight
            + medicalHelpUrgencyPoints * medicalHelpUrgencyWeight
            + behaviourPoints * behaviorWeight
            + ageCategoryPoints * ageWeight
            + daysAwaiting * daysAwaitingWeight;
        
        return priority;
    }
}