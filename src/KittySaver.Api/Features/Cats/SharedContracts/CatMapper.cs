using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Common.Primitives.Enums;
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
                MedicalHelpUrgency = MedicalHelpUrgency.FromValue(entity.MedicalHelpUrgency)
                    .ToString(),
                AgeCategory = AgeCategory.FromValue(entity.AgeCategory)
                    .ToString(),
                Behavior = Behavior.FromValue(entity.Behavior)
                    .ToString(),
                HealthStatus = HealthStatus.FromValue(entity.HealthStatus)
                    .ToString(),
                PriorityScore = entity.PriorityScore,
                IsAssignedToAdvertisement = entity.AdvertisementId.HasValue,
                IsThumbnailUploaded = entity.IsThumbnailUploaded
            }
        );
}