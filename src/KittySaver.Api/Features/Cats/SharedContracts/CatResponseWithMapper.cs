using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Common.Primitives.Enums;

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
    public required List<Link> Links { get; init; }
}

public static class CatResponseMapper
{
    private static List<Link> AddLinks(CatReadModel catResponse, ILinkService linkService)
    {
        List<Link> links =
        [
            linkService.Generate(
                endpointInfo: EndpointNames.GetCat,
                routeValues: new { id = catResponse.Id, personId = catResponse.PersonId },
                isSelf: true),

            linkService.Generate(
                endpointInfo: EndpointNames.UpdateCat,
                routeValues: new { id = catResponse.Id, personId = catResponse.PersonId }),

            linkService.Generate(
                endpointInfo: EndpointNames.DeleteCat,
                routeValues: new { id = catResponse.Id, personId = catResponse.PersonId })
        ];

        if (catResponse.AdvertisementId is not null)
        {
            links.Add(linkService.Generate(
                endpointInfo: EndpointNames.GetAdvertisement,
                routeValues: new { id = catResponse.AdvertisementId }));
        }
        
        return links;
    }
    public static IQueryable<CatResponse> ProjectToDto(
        this IQueryable<CatReadModel> cats,
        ILinkService linkService)
        => cats.Select(entity => new CatResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                AdditionalRequirements = entity.AdditionalRequirements,
                IsCastrated = entity.IsCastrated,
                IsAdopted = entity.IsAdopted,
                MedicalHelpUrgency = MedicalHelpUrgency.FromValue(entity.MedicalHelpUrgency).ToString(),
                AgeCategory = AgeCategory.FromValue(entity.AgeCategory).ToString(),
                Behavior = Behavior.FromValue(entity.Behavior).ToString(),
                HealthStatus = HealthStatus.FromValue(entity.HealthStatus).ToString(),
                PriorityScore = entity.PriorityScore,
                IsAssignedToAdvertisement = entity.AdvertisementId.HasValue,
                Links = AddLinks(entity, linkService)
            }
        );
}