using KittySaver.Api.Persistence.ReadRelated.ReadModels;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;

namespace KittySaver.Api.Features.Cats.SharedContracts;

public static class CatMapper
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
                MedicalHelpUrgency = MedicalHelpUrgency.FromNameOrValue(entity.MedicalHelpUrgency, true).ToString(),
                AgeCategory = AgeCategory.FromNameOrValue(entity.AgeCategory, true).ToString(),
                Behavior = Behavior.FromNameOrValue(entity.Behavior, true).ToString(),
                HealthStatus = HealthStatus.FromNameOrValue(entity.HealthStatus, true).ToString(),
                PriorityScore = entity.PriorityScore,
                IsAssignedToAdvertisement = entity.AdvertisementId.HasValue,
                IsThumbnailUploaded = entity.IsThumbnailUploaded
            }
        );
}