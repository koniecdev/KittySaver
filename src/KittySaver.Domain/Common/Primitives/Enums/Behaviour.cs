using Ardalis.SmartEnum;
using KittySaver.Domain.Common.Primitives.Enums.Common;

namespace KittySaver.Domain.Common.Primitives.Enums;

public sealed class Behavior : SmartEnum<Behavior>, IScoreCompound
{
    public static readonly Behavior Friendly = new(nameof(Friendly), 1, 10);
    public static readonly Behavior Unfriendly = new(nameof(Unfriendly), 2, 5);
    public int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private Behavior(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
