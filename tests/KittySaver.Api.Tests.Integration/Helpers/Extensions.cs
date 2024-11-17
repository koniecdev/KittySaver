using System.Text;

namespace KittySaver.Api.Tests.Integration.Helpers;

public static class Extensions
{
    public static string InsertSpacesIntoCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var result = new StringBuilder();
        foreach (var character in input)
        {
            if (char.IsUpper(character) && result.Length > 0)
            {
                result.Append(' ');
            }
            result.Append(character);
        }
        return result.ToString();
    }
}