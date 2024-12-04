using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed class CatReadModel
{
    public required Guid Id { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTimeOffset CreatedOn { get; init; }
    public required string? LastModificationBy { get; init; }
    public required DateTimeOffset? LastModificationOn { get; init; }
    public required Guid PersonId { get; init; }
    public required int MedicalHelpUrgency { get; init; }
    public required int AgeCategory { get; init; }
    public required int Behavior { get; init; }
    public required int HealthStatus { get; init; }
    public required bool IsCastrated { get; init; }
    public required bool IsAdopted { get; init; }
    public required double PriorityScore { get; init; }
    public required string AdditionalRequirements { get; init; }
    public required string Name { get; init; }
    public Guid? AdvertisementId { get; init; }
    public AdvertisementReadModel? Advertisement { get; private set; }
}