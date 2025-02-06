using KittySaver.Shared.Hateoas;

namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public sealed class CatResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid? AdvertisementId { get; init; }
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