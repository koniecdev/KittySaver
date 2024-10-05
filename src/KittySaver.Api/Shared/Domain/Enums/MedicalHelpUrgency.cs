using KittySaver.Api.Shared.Domain.Enums.Common;

namespace KittySaver.Api.Shared.Domain.Enums;
public sealed class MedicalHelpUrgency : SmartEnum<MedicalHelpUrgency>, IScoreCompound
{
    public static readonly MedicalHelpUrgency HaveToSeeVet = new(nameof(HaveToSeeVet), 1, 10);
    public static readonly MedicalHelpUrgency ShouldSeeVet = new(nameof(ShouldSeeVet), 2, 5);
    public static readonly MedicalHelpUrgency NoNeed = new(nameof(NoNeed), 3, 1);
    public int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private MedicalHelpUrgency(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
