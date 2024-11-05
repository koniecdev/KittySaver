using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Cat : AuditableEntity
{
    public static Cat Create(
        ICatPriorityCalculator calculator,
        Person person,
        string name,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus,
        bool isCastrated = false,
        bool isInNeedOfSeeingVet = false,
        string? additionalRequirements = null)
    {
        Cat cat = new(
            person: person,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            isCastrated: isCastrated,
            isInNeedOfSeeingVet: isInNeedOfSeeingVet,
            additionalRequirements: additionalRequirements);
        cat.PriorityScore = cat.CalculatePriorityScore(calculator);
        person.AddCat(cat);
        return cat;
    }

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Cat()
    {
        Person = null!;
    }

    [SetsRequiredMembers]
    private Cat(
        Person person,
        string name,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus,
        bool isCastrated,
        bool isInNeedOfSeeingVet,
        string? additionalRequirements)
    {
        PersonId = person.Id;
        Person = person;
        Name = name;
        MedicalHelpUrgency = medicalHelpUrgency;
        AgeCategory = ageCategory;
        Behavior = behavior;
        HealthStatus = healthStatus;
        IsCastrated = isCastrated;
        IsInNeedOfSeeingVet = isInNeedOfSeeingVet;
        AdditionalRequirements = additionalRequirements;
    }

    private string _name = null!;
    private string? _additionalRequirements;
    private readonly Guid _personId;
    private Guid? _advertisementId;

    public required string Name
    {
        get => _name;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Name));
            if (value.Length > Constraints.NameMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Name),
                    $"Cat Name cannot be longer than {Constraints.NameMaxLength} characters.");
            }

            _name = value;
        }
    }

    public string? AdditionalRequirements
    {
        get => _additionalRequirements;
        set
        {
            if (value?.Length > Constraints.AdditionalRequirementsMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(AdditionalRequirements),
                    $"Cat Additional requirements cannot be longer than {Constraints.AdditionalRequirementsMaxLength} characters.");
            }

            _additionalRequirements = value;
        }
    }

    public bool IsAdopted { get; private set; }
    public required bool IsCastrated { get; set; }
    public required bool IsInNeedOfSeeingVet { get; set; }
    public required MedicalHelpUrgency MedicalHelpUrgency { get; set; }
    public required AgeCategory AgeCategory { get; set; }
    public required Behavior Behavior { get; set; }
    public required HealthStatus HealthStatus { get; set; }

    public Guid PersonId
    {
        get => _personId;
        private init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Provided person id is empty", nameof(PersonId));
            }

            _personId = value;
        }
    }

    public Person Person { get; private init; }

    public Guid? AdvertisementId
    {
        get => _advertisementId;
        private set
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Provided advertisement id is empty", nameof(AdvertisementId));
            }

            _advertisementId = value;
        }
    }

    public Advertisement? Advertisement { get; private set; }

    public void AssignAdvertisement(Advertisement advertisement)
    {
        if (AdvertisementId is not null || Advertisement is not null)
        {
            throw new InvalidOperationException("You can not assign advertisement to cat, that is already assigned to some other advertisement.");
        }
        AdvertisementId = advertisement.Id;
        Advertisement = advertisement;
    }
    
    public void UnassignAdvertisement()
    {
        AdvertisementId = null;
        Advertisement = null;
    }
    
    public double PriorityScore { get; private set; }

    private double CalculatePriorityScore(ICatPriorityCalculator calculator)
    {
        double priority = calculator.Calculate(this);
        return priority;
    }

    public void ReCalculatePriorityScore(ICatPriorityCalculator calculator)
    {
        PriorityScore = CalculatePriorityScore(calculator);
    }

    public void MarkAsAdopted()
    {
        IsAdopted = true;
    }

    public static class Constraints
    {
        public const int NameMaxLength = 100;
        public const int AdditionalRequirementsMaxLength = 1000;
    }
}

internal sealed class CatConfiguration : IEntityTypeConfiguration<Cat>
{
    public void Configure(EntityTypeBuilder<Cat> builder)
    {
        builder.ToTable("Cats");
        
        builder.Property(x => x.Id).ValueGeneratedNever();
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PersonId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(Cat.Constraints.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.AdditionalRequirements)
            .HasMaxLength(Cat.Constraints.AdditionalRequirementsMaxLength);

        builder.Property(x => x.AgeCategory)
            .IsRequired();

        builder.Property(x => x.Behavior)
            .IsRequired();

        builder.Property(x => x.HealthStatus)
            .IsRequired();

        builder.Property(x => x.PriorityScore)
            .IsRequired();
    }
}