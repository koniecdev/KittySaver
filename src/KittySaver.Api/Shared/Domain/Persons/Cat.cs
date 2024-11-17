using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Shared.Domain.Common.Primitives;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.Persons;

public sealed class Cat : AuditableEntity
{
    private string? _additionalRequirements;
    private readonly Guid _personId;
    private Guid? _advertisementId;
    private double _priorityScore;

    public static Cat Create(
        ICatPriorityCalculatorService priorityScoreCalculator,
        Person person,
        CatName name,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus,
        Description additionalRequirements,
        bool isCastrated = false)
    {
        Cat cat = new(
            personId: person.Id,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            isCastrated: isCastrated,
            additionalRequirements: additionalRequirements);
        cat.RecalculatePriorityScore(priorityScoreCalculator);
        person.AddCat(cat);
        return cat;
    }

    /// <remarks>
    /// Required by EF Core, and should never be used by programmer as it bypasses business rules.
    /// </remarks>
    private Cat()
    {
        Name = null!;
        AdditionalRequirements = null!;
        HealthStatus = null!;
        MedicalHelpUrgency = null!;
        Behavior = null!;
        AgeCategory = null!;
    }

    [SetsRequiredMembers]
    private Cat(
        Guid personId,
        CatName name,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus,
        Description additionalRequirements,
        bool isCastrated)
    {
        PersonId = personId;
        Name = name;
        MedicalHelpUrgency = medicalHelpUrgency;
        AgeCategory = ageCategory;
        Behavior = behavior;
        HealthStatus = healthStatus;
        IsCastrated = isCastrated;
        AdditionalRequirements = additionalRequirements;
    }
    
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
    public CatName Name { get; set; }
    public Description AdditionalRequirements { get; set; }
    public MedicalHelpUrgency MedicalHelpUrgency { get; internal set; }
    public AgeCategory AgeCategory { get; internal set; }
    public Behavior Behavior { get; internal set; }
    public HealthStatus HealthStatus { get; internal set; }
    public required bool IsCastrated { get; set; }
    public bool IsAdopted { get; private set; }

    public double PriorityScore
    {
        get => _priorityScore;
        private set
        {
            if (value == 0)
            {
                throw new ArgumentException("PriorityScore can not be zero, probably something went wrong.", nameof(PriorityScore));
            }
            _priorityScore = value;
        }
    }

    public void MarkAsAdopted()
    {
        IsAdopted = true;
    }
    
    /// <remarks>
    /// Only for use within Person aggregate
    /// </remarks>
    internal void RecalculatePriorityScore(ICatPriorityCalculatorService calculator)
    {
        double priority = calculator.Calculate(this);
        PriorityScore = priority;
    }
    
    /// <remarks>
    /// Only for use within Person aggregate
    /// </remarks>
    internal void AssignAdvertisement(Guid advertisementId)
    {
        if (AdvertisementId is not null)
        {
            throw new InvalidOperationException("You can not assign advertisement to cat, that is already assigned to some other advertisement.");
        }
        AdvertisementId = advertisementId;
    }
    
    /// <remarks>
    /// Only for use within Person aggregate
    /// </remarks>
    internal void UnassignAdvertisement()
    {
        if (AdvertisementId is null)
        {
            throw new InvalidOperationException("You can not unassign advertisement from cat, that do not even have advertisement assigned.");
        }
        AdvertisementId = null;
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

        builder.ComplexProperty(x => x.Name, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(CatName.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.AdditionalRequirements, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasMaxLength(Description.MaxLength)
                .IsRequired();
        });

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