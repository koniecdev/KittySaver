using KittySaver.Shared.Common.Enums.Common;

namespace KittySaver.Shared.Common.Enums;
public sealed class MedicalHelpUrgency : SmartEnumBase<MedicalHelpUrgency>
{
    public static readonly MedicalHelpUrgency HaveToSeeVet = new(nameof(HaveToSeeVet), 0, 10);
    public static readonly MedicalHelpUrgency ShouldSeeVet = new(nameof(ShouldSeeVet), 1, 5);
    public static readonly MedicalHelpUrgency NoNeed = new(nameof(NoNeed), 2, 1);
    public static int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private MedicalHelpUrgency(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
