using Ardalis.SmartEnum;
using KittySaver.Domain.Common.Primitives.Enums.Common;

namespace KittySaver.Domain.Common.Primitives.Enums;

public sealed class Behavior : SmartEnumBase<Behavior>
{
    public static readonly Behavior Friendly = new(nameof(Friendly), 0, 10);
    public static readonly Behavior Unfriendly = new(nameof(Unfriendly), 1, 5);
    public static readonly Behavior Shy = new(nameof(Shy), 2, 3);
    public static readonly Behavior Aggressive = new(nameof(Aggressive), 3, 1);
    public static int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private Behavior(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
