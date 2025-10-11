using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModularMachineryStructureOptimizer.JsonConverters;

public class MachineJsonConverter : JsonConverter<Machine>
{
    public override Machine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");
        Machine machine = new()
        {
            Parts = null,
            DynamicPatterns = null,
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
            else if ("dynamic-patterns".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                machine.DynamicPatterns = JsonSerializer.Deserialize<List<Machine.DynamicPattern>?>(ref reader, options);
            else if (JsonNode.Parse(ref reader) is JsonNode node)
                (machine.Extra ??= [])[propertyName] = node;
        }
        throw new JsonException("Expected EndObject token");
    }

    public override void Write(Utf8JsonWriter writer, Machine value, JsonSerializerOptions options)
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
        if (value.DynamicPatterns?.Count is > 0)
        {
            writer.WritePropertyName("dynamic-patterns");
            JsonSerializer.Serialize(writer, value.DynamicPatterns, options);
        }
        writer.WriteEndObject();
    }
}