using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KittySaver.Tests.Shared;

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
    
    public static async Task<TId> GetIdResponseFromResponseMessageAsync<TId>(this HttpResponseMessage responseMessage)
    {
        TId response = await responseMessage.Content.ReadFromJsonAsync<TId>() ?? throw new JsonException();
        return response;
    }
    public static async Task<TResponse> GetResponseFromResponseMessageAsync<TResponse>(this HttpResponseMessage responseMessage)
    {
        TResponse response = await responseMessage.Content.ReadFromJsonAsync<TResponse>()
                             ?? throw new JsonException();
        return response;
    }
}