using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Common.Primitives.Enums;

namespace KittySaver.Api.Features.Cats.SharedContracts;

public sealed class CatResponse : IHateoasCatResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid? AdvertisementId { get; init; }
    public required string Name { get; init; }
    public required string? AdditionalRequirements { get; init; }
    public required bool IsCastrated { get; init; }
    public required bool IsAdopted { get; init; }
    public required bool IsAssignedToAdvertisement { get; init; }
    public required string MedicalHelpUrgency { get; init; }
    public required string AgeCategory { get; init; }
    public required string Behavior { get; init; }
    public required string HealthStatus { get; init; }
    public required double PriorityScore { get; init; }
    public ICollection<Link> Links { get; set; } = new List<Link>();
}

public static class CatResponseMapper
{
    public static IQueryable<CatResponse> ProjectToDto(
        this IQueryable<CatReadModel> cats)
        => cats.Select(entity => new CatResponse
            {
                Id = entity.Id,
                PersonId = entity.PersonId,
                AdvertisementId = entity.AdvertisementId,
                Name = entity.Name,
                AdditionalRequirements = entity.AdditionalRequirements,
                IsCastrated = entity.IsCastrated,
                IsAdopted = entity.IsAdopted,
                MedicalHelpUrgency = MedicalHelpUrgency.FromValue(entity.MedicalHelpUrgency).ToString(),
                AgeCategory = AgeCategory.FromValue(entity.AgeCategory).ToString(),
                Behavior = Behavior.FromValue(entity.Behavior).ToString(),
                HealthStatus = HealthStatus.FromValue(entity.HealthStatus).ToString(),
                PriorityScore = entity.PriorityScore,
                IsAssignedToAdvertisement = entity.AdvertisementId.HasValue
            }
        );
}