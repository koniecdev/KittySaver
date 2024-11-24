using Ardalis.SmartEnum;
using KittySaver.Domain.Common.Primitives.Enums.Common;

namespace KittySaver.Domain.Common.Primitives.Enums;
public sealed class AgeCategory : SmartEnum<AgeCategory>, IScoreCompound
{
    public static readonly AgeCategory Baby = new(nameof(Baby), 1, 10);
    public static readonly AgeCategory Adult = new(nameof(Adult), 2, 5);
    public static readonly AgeCategory Senior = new(nameof(Senior), 3, 1);
    public int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private AgeCategory(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
