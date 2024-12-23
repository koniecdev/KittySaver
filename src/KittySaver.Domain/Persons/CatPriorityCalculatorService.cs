namespace KittySaver.Domain.Persons;

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
        int healthStatusPoints = cat.HealthStatus.MaxScorePoints - cat.HealthStatus.ScorePoints;
        int behaviourPoints = cat.Behavior.MaxScorePoints - cat.Behavior.ScorePoints;
        int medicalHelpUrgencyPoints = cat.MedicalHelpUrgency.MaxScorePoints - cat.MedicalHelpUrgency.ScorePoints;
        int ageCategoryPoints = cat.AgeCategory.MaxScorePoints - cat.AgeCategory.ScorePoints;

        return healthStatusPoints * Weights.Health
               + medicalHelpUrgencyPoints * Weights.MedicalUrgency
               + behaviourPoints * Weights.Behavior
               + ageCategoryPoints * Weights.Age
               + Weights.BaseScore;
    }
}