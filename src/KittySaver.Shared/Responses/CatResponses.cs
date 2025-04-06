using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Shared.Responses;

public sealed class CatResponse : IHateoasCatResponse
{
    public required CatId Id { get; init; }
    public required PersonId PersonId { get; init; }
    public required AdvertisementId? AdvertisementId { get; init; }
    public required string Name { get; init; }
    public required string? AdditionalRequirements { get; init; }
    public required bool IsCastrated { get; init; }
    public required bool IsAdopted { get; init; }
    public required bool IsThumbnailUploaded { get; init; }
    public required bool IsAssignedToAdvertisement { get; init; }
    public required string MedicalHelpUrgency { get; init; }
    public required string AgeCategory { get; init; }
    public required string Behavior { get; init; }
    public required string HealthStatus { get; init; }
    public required double PriorityScore { get; init; }
    public ICollection<Link> Links { get; set; } = new List<Link>();
}