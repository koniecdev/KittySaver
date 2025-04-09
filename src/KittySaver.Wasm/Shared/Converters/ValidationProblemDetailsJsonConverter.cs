using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Wasm.Shared.Converters;

public class ValidationProblemDetailsConverter : JsonConverter<ValidationProblemDetails>
{
    public override ValidationProblemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ValidationProblemDetails problemDetails = new();
        
        using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement root = jsonDoc.RootElement;
        
        if (root.TryGetProperty("status", out JsonElement statusElement))
            problemDetails.Status = statusElement.GetInt32();
            
        if (root.TryGetProperty("title", out JsonElement titleElement))
            problemDetails.Title = titleElement.GetString();
            
        if (root.TryGetProperty("type", out JsonElement typeElement))
            problemDetails.Type = typeElement.GetString();

        if (!root.TryGetProperty("errors", out JsonElement errorsElement))
        {
            return problemDetails;
        }
        
        foreach (JsonProperty property in errorsElement.EnumerateObject())
        {
            string?[] errorMessages = property.Value.EnumerateArray()
                .Select(e => e.GetString())
                .Where(e => e != null)
                .ToArray();
                
            problemDetails.Errors.Add(property.Name, errorMessages);
        }

        return problemDetails;
    }
    
    public override void Write(Utf8JsonWriter writer, ValidationProblemDetails value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}