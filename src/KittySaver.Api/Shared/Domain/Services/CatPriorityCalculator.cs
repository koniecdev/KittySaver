using KittySaver.Api.Shared.Domain.Entites;

namespace KittySaver.Api.Shared.Domain.Services;

public interface ICatPriorityCalculator
{
    public double Calculate(Cat cat);
}

public class DefaultCatPriorityCalculator : ICatPriorityCalculator
{
    public double Calculate(Cat cat)
    {
        const double healthWeight = 2.0;
        const double medicalHelpUrgencyWeight = 2.0;
        const double behaviorWeight = 1.5;
        const double ageWeight = 1.2;
    
        int healthStatusPoints = cat.HealthStatus.MaxScorePoints - cat.HealthStatus.ScorePoints;
        int behaviourPoints = cat.Behavior.MaxScorePoints - cat.Behavior.ScorePoints;
        int medicalHelpUrgencyPoints =  cat.MedicalHelpUrgency.MaxScorePoints - cat.MedicalHelpUrgency.ScorePoints;
        int ageCategoryPoints = cat.AgeCategory.MaxScorePoints - cat.AgeCategory.ScorePoints;

        double priority =
            healthStatusPoints * healthWeight
            + medicalHelpUrgencyPoints * medicalHelpUrgencyWeight
            + behaviourPoints * behaviorWeight
            + ageCategoryPoints * ageWeight;
        
        return priority;
    }
}