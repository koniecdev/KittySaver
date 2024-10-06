using KittySaver.Api.Shared.Domain.Entites;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats.SharedContracts;

public sealed class CatResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public required string Name { get; init; }
    public required string? AdditionalRequirements { get; init; }
    public required bool IsCastrated { get; init; }
    public required bool IsInNeedOfSeeingVet { get; init; }
    public required string MedicalHelpUrgencyName { get; init; }
    public required string AgeCategoryName { get; init; }
    public required string BehaviorName { get; init; }
    public required string HealthStatusName { get; init; }
}

[Mapper]
public static partial class CatResponseMapper
{
    public static partial IQueryable<CatResponse> ProjectToDto(
        this IQueryable<Cat> persons);
}