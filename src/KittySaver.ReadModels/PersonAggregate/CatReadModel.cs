using KittySaver.Shared.TypedIds;

// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.ReadModels.EntityFramework.PersonAggregate;

public sealed class CatReadModel
{
    public required CatId Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required string MedicalHelpUrgency { get; init; }
    public required string AgeCategory { get; init; }
    public required string Behavior { get; init; }
    public required string HealthStatus { get; init; }
    public required bool IsCastrated { get; init; }
    public required bool IsAdopted { get; init; }
    public required bool IsThumbnailUploaded { get; init; }
    public required double PriorityScore { get; init; }
    public required string AdditionalRequirements { get; init; }
    public required string Name { get; init; }
    public required PersonId PersonId { get; init; }
    public PersonReadModel Person { get; private init; } = null!;
    public required AdvertisementId? AdvertisementId { get; init; }
    public AdvertisementReadModel? Advertisement { get; private init; }
}