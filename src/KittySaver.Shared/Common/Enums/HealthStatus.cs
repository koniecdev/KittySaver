using Ardalis.SmartEnum;
using KittySaver.Domain.Common.Primitives.Enums.Common;

namespace KittySaver.Domain.Common.Primitives.Enums;

public sealed class HealthStatus : SmartEnumBase<HealthStatus>
{
    public static readonly HealthStatus Good = new(nameof(Good), 0, 10);
    public static readonly HealthStatus Unknown = new(nameof(Unknown), 1, 7);
    public static readonly HealthStatus ChronicMinor = new(nameof(ChronicMinor), 2, 5);
    public static readonly HealthStatus ChronicSerious = new(nameof(ChronicSerious), 3, 2);
    public static readonly HealthStatus Terminal = new(nameof(Terminal), 4, 1);
    
    public static int MaxScorePoints => 10;
    public int ScorePoints { get; }

    private HealthStatus(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}