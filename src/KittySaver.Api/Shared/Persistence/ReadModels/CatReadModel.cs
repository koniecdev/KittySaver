namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed record CatReadModel(
    Guid Id,
    string CreatedBy,
    DateTimeOffset CreatedOn,
    string? LastModificationBy,
    DateTimeOffset? LastModificationOn,
    Guid PersonId,
    Guid? AdvertisementId,
    AdvertisementReadModel? Advertisement,
    int MedicalHelpUrgency,
    int AgeCategory,
    int Behavior,
    int HealthStatus,
    bool IsCastrated,
    bool IsAdopted,
    double PriorityScore,
    string AdditionalRequirements,
    string Name
);