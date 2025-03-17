namespace KittySaver.Shared.Requests;

public sealed record CreateCatRequest(
    string Name,
    bool IsCastrated,
    string MedicalHelpUrgency,
    string AgeCategory,
    string Behavior,
    string HealthStatus,
    string? AdditionalRequirements = null);
    
public sealed record UpdateCatRequest(
    string Name,
    bool IsCastrated,
    string MedicalHelpUrgency,
    string AgeCategory,
    string Behavior,
    string HealthStatus,
    string? AdditionalRequirements = null);