using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Shared;

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
    
    public static async Task<ApiResponses.CreatedWithIdResponse> GetIdResponseFromResponseMessageAsync(this HttpResponseMessage responseMessage)
    {
        ApiResponses.CreatedWithIdResponse response =
            await responseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        return response;
    }
}