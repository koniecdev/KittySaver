namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed record CatReadModel(
    Guid Id,
    string CreatedBy,
    DateTimeOffset CreatedOn,
    string? LastModificationBy,
    DateTimeOffset? LastModificationOn,
    Guid PersonId,
    PersonReadModel Person,
    Guid? AdvertisementId,
    AdvertisementReadModel? Advertisement,
    string MedicalHelpUrgency,
    string AgeCategory,
    string Behavior,
    string HealthStatus,
    string IsCastrated,
    string IsAdopted,
    string PriorityScore,
    string AdditionalRequirements,
    string Name
);