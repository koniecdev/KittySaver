using KittySaver.Domain.Persons;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats.SharedContracts;

public sealed class CatResponse
{
    public required Guid Id { get; init; }
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
}

[Mapper]
public static partial class CatResponseMapper
{
    public static IQueryable<CatResponse> ProjectToDto(
        this IQueryable<Cat> cats) =>
        cats.Select(x => new CatResponse
            {
                Id = x.Id,
                Name = x.Name,
                AdditionalRequirements = x.AdditionalRequirements,
                IsCastrated = x.IsCastrated,
                IsAdopted = x.IsAdopted,
                MedicalHelpUrgency = x.MedicalHelpUrgency.ToString(),
                AgeCategory = x.AgeCategory.ToString(),
                Behavior = x.Behavior.ToString(),
                HealthStatus = x.HealthStatus.ToString(),
                PriorityScore = x.PriorityScore,
                IsAssignedToAdvertisement = x.AdvertisementId.HasValue
            }
        );
}