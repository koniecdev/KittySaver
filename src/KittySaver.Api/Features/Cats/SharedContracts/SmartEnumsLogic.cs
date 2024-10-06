using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Enums.Common;

namespace KittySaver.Api.Features.Cats.SharedContracts;

public interface ICatSmartEnumsRequest
{
    public string MedicalHelpUrgencyName { get; }
    public string AgeCategoryName { get; }
    public string BehaviorName { get; }
    public string HealthStatusName { get; }
}

public static class CatSmartEnumsHelper
{
    public static void ValidateAndRetrieve(
        this ICatSmartEnumsRequest request,
        out MedicalHelpUrgency medicalHelpUrgency,
        out AgeCategory ageCategory,
        out Behavior behavior,
        out HealthStatus healthStatus)
    {
        List<(string propertyName, string attemptedValue)> smartEnumsFailures = [];
        if (!MedicalHelpUrgency.TryFromName(request.MedicalHelpUrgencyName, true,
                out medicalHelpUrgency))
        {
            smartEnumsFailures.Add((nameof(request.MedicalHelpUrgencyName), request.MedicalHelpUrgencyName));
        }

        if (!AgeCategory.TryFromName(request.AgeCategoryName, true,
                out ageCategory))
        {
            smartEnumsFailures.Add((nameof(request.AgeCategoryName), request.AgeCategoryName));
        }

        if (!Behavior.TryFromName(request.BehaviorName, true,
                out behavior))
        {
            smartEnumsFailures.Add((nameof(request.BehaviorName), request.BehaviorName));
        }

        if (!HealthStatus.TryFromName(request.HealthStatusName, true,
                out healthStatus))
        {
            smartEnumsFailures.Add((nameof(request.HealthStatusName), request.HealthStatusName));
        }

        if (smartEnumsFailures.Count > 0)
        {
            throw new SmartEnumsExceptions.InvalidValueException(smartEnumsFailures);
        }
    }
}