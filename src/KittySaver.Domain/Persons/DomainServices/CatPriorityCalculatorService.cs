using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Common.Enums;

namespace KittySaver.Domain.Persons.DomainServices;

public interface ICatPriorityCalculatorService
{
    public double Calculate(Cat cat);
}

public class DefaultCatPriorityCalculatorService : ICatPriorityCalculatorService
{
    private static class Weights
    {
        public const double Health = 2.0;
        public const double MedicalUrgency = 2.0;
        public const double Behavior = 1.5;
        public const double Age = 1.2;
        public const double BaseScore = 0.1;
    }

    public double Calculate(Cat cat)
    {
        int healthStatusPoints = HealthStatus.MaxScorePoints - cat.HealthStatus.ScorePoints;
        int behaviourPoints = Behavior.MaxScorePoints - cat.Behavior.ScorePoints;
        int medicalHelpUrgencyPoints = MedicalHelpUrgency.MaxScorePoints - cat.MedicalHelpUrgency.ScorePoints;
        int ageCategoryPoints = AgeCategory.MaxScorePoints - cat.AgeCategory.ScorePoints;

        return healthStatusPoints * Weights.Health
               + medicalHelpUrgencyPoints * Weights.MedicalUrgency
               + behaviourPoints * Weights.Behavior
               + ageCategoryPoints * Weights.Age
               + Weights.BaseScore;
    }
}