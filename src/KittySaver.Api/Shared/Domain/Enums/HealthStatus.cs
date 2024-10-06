using KittySaver.Api.Shared.Domain.Enums.Common;

namespace KittySaver.Api.Shared.Domain.Enums;

public sealed class HealthStatus : SmartEnum<HealthStatus>, IScoreCompound
{
    public static readonly HealthStatus Good = new(nameof(Good), 1, 10);
    public static readonly HealthStatus Poor = new(nameof(Poor), 2, 5);
    public static readonly HealthStatus Critical = new(nameof(Critical), 3, 1);
    public int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private HealthStatus(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
