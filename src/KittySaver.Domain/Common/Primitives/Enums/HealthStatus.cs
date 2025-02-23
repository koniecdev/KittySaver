using Ardalis.SmartEnum;
using KittySaver.Domain.Common.Primitives.Enums.Common;

namespace KittySaver.Domain.Common.Primitives.Enums;

public sealed class HealthStatus : SmartEnumBase<HealthStatus>
{
    public static readonly HealthStatus Good = new(nameof(Good), 0, 10);
    public static readonly HealthStatus Poor = new(nameof(Poor), 1, 5);
    public static readonly HealthStatus Critical = new(nameof(Critical), 2, 1);
    public static int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private HealthStatus(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
