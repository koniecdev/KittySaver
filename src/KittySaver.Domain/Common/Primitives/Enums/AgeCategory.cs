using Ardalis.SmartEnum;
using KittySaver.Domain.Common.Primitives.Enums.Common;

namespace KittySaver.Domain.Common.Primitives.Enums;
public sealed class AgeCategory : SmartEnumBase<AgeCategory>
{
    public static readonly AgeCategory Baby = new(nameof(Baby), 0, 10);
    public static readonly AgeCategory Adult = new(nameof(Adult), 1, 5);
    public static readonly AgeCategory Senior = new(nameof(Senior), 2, 1);
    public static int MaxScorePoints => 10;
    public int ScorePoints { get; }
    private AgeCategory(string name, int value, int scorePoints) : base(name, value)
    {
        ScorePoints = scorePoints;
    }
}
