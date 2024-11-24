using KittySaver.Domain.Common.Primitives.Enums;

namespace KittySaver.Api.Features.Cats.SharedContracts;

public interface ICatSmartEnumsRequest
{
    public string MedicalHelpUrgency { get; }
    public string AgeCategory { get; }
    public string Behavior { get; }
    public string HealthStatus { get; }
}

public static class CatSmartEnumsRequestExtensions
{
    public static void RetrieveSmartEnumsFromNames(
        this ICatSmartEnumsRequest request,
        out (bool mappedSuccessfully, MedicalHelpUrgency value) medicalHelpUrgencyResults,
        out (bool mappedSuccessfully, AgeCategory value) ageCategoryResults,
        out (bool mappedSuccessfully, Behavior value) behaviorResults,
        out (bool mappedSuccessfully, HealthStatus value) healthStatusResults)
    {
        medicalHelpUrgencyResults =
            MedicalHelpUrgency.TryFromName(request.MedicalHelpUrgency, true,
                out MedicalHelpUrgency medicalHelpUrgency)
                ? (true, medicalHelpUrgency)
                : (false, null!);
        
        ageCategoryResults =
            AgeCategory.TryFromName(request.AgeCategory, true,
                out AgeCategory ageCategory)
                ? (true, ageCategory)
                : (false, null!);

        behaviorResults =
            Behavior.TryFromName(request.Behavior, true,
                out Behavior behavior)
                ? (true, behavior)
                : (false, null!);

        healthStatusResults =
            HealthStatus.TryFromName(request.HealthStatus, true,
                out HealthStatus healthStatus)
                ? (true, healthStatus)
                : (false, null!);
    }
}