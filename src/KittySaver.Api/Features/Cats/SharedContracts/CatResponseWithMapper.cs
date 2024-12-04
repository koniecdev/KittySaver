using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Common.Primitives.Enums;
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

public static class CatResponseMapper
{
    public static IQueryable<CatResponse> ProjectToDto(this IQueryable<CatReadModel> cats)
        => cats.Select(x => new CatResponse
            {
                Id = x.Id,
                Name = x.Name,
                AdditionalRequirements = x.AdditionalRequirements,
                IsCastrated = x.IsCastrated,
                IsAdopted = x.IsAdopted,
                MedicalHelpUrgency = MedicalHelpUrgency.FromValue(x.MedicalHelpUrgency).ToString(),
                AgeCategory = AgeCategory.FromValue(x.AgeCategory).ToString(),
                Behavior = Behavior.FromValue(x.Behavior).ToString(),
                HealthStatus = HealthStatus.FromValue(x.HealthStatus).ToString(),
                PriorityScore = x.PriorityScore,
                IsAssignedToAdvertisement = x.AdvertisementId.HasValue
            }
        );
}