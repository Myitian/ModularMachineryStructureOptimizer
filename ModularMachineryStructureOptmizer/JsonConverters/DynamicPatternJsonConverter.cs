using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModularMachineryStructureOptmizer.JsonConverters;

public class DynamicPatternJsonConverter : JsonConverter<Machine.DynamicPattern>
{
    public override Machine.DynamicPattern Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");
        Machine.DynamicPattern machine = new()
        {
            Parts = null,
            PartsEnd = null,
            Extra = null
        };
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return machine;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");
            string? propertyName = reader.GetString();
            reader.Read();
            if (propertyName is null)
                throw new JsonException();
            else if ("parts".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                machine.Parts = JsonSerializer.Deserialize<List<Machine.Part>?>(ref reader, options);
            else if ("parts-end".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                machine.PartsEnd = JsonSerializer.Deserialize<List<Machine.Part>?>(ref reader, options);
            else if (JsonNode.Parse(ref reader) is JsonNode node)
                (machine.Extra ??= [])[propertyName] = node;
        }
        throw new JsonException("Expected EndObject token");
    }

    public override void Write(Utf8JsonWriter writer, Machine.DynamicPattern value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Extra?.Count is > 0)
        {
            foreach ((string key, JsonNode? objValue) in value.Extra)
            {
                if (objValue is null)
                    continue;
                writer.WritePropertyName(key);
                objValue.WriteTo(writer, options);
            }
        }
        if (value.Parts?.Count is > 0)
        {
            writer.WritePropertyName("parts");
            JsonSerializer.Serialize(writer, value.Parts, options);
        }
        if (value.PartsEnd?.Count is > 0)
        {
            writer.WritePropertyName("dynamic-patterns");
            JsonSerializer.Serialize(writer, value.PartsEnd, options);
        }
        writer.WriteEndObject();
    }
}