using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModularMachineryStructureOptimizer.JsonConverters;

public class PartJsonConverter : JsonConverter<Machine.Part>
{
    public override Machine.Part Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");
        Machine.Part part = new()
        {
            X = [],
            Y = [],
            Z = [],
            Elements = [],
            Extra = null
        };
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (!part.IsValidPart)
                    throw new JsonException("Invalid structure!");
                return part;
            }
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");
            string? propertyName = reader.GetString();
            reader.Read();
            if (propertyName is null)
                throw new JsonException();
            else if ("x".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                part.X = ReadIntArray(ref reader);
            else if ("y".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                part.Y = ReadIntArray(ref reader);
            else if ("z".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                part.Z = ReadIntArray(ref reader);
            else if ("elements".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                part.Elements = ReadStringSet(ref reader);
            else if (JsonNode.Parse(ref reader) is JsonNode node)
                (part.Extra ??= [])[propertyName] = node;
        }
        throw new JsonException("Expected EndObject token");
    }
    private static int[] ReadIntArray(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return [reader.GetInt32()];
            case JsonTokenType.StartArray:
                {
                    List<int> list = new List<int>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType == JsonTokenType.Number)
                            list.Add(reader.GetInt32());
                        else
                            throw UnexpectedTokenException(reader.TokenType);
                    }
                    return [.. list];
                }
            default:
                throw UnexpectedTokenException(reader.TokenType);
        }

    }
    private static SortedSet<string> ReadStringSet(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return [reader.GetString()];
            case JsonTokenType.StartArray:
                {
                    SortedSet<string> set = [];
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType == JsonTokenType.String)
                            set.Add(reader.GetString()!);
                        else
                            throw UnexpectedTokenException(reader.TokenType);
                    }
                    return set;
                }
            default:
                throw UnexpectedTokenException(reader.TokenType);
        }
    }
    private static JsonException UnexpectedTokenException(JsonTokenType tokenType)
    {
        return new JsonException($"Unexpected token type: {tokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Machine.Part value, JsonSerializerOptions options)
    {
        if (!value.IsValidPart)
            throw new JsonException("Invalid structure!");
        writer.WriteStartObject();
        WriteIntArray(writer, "x", value.X);
        WriteIntArray(writer, "y", value.Y);
        WriteIntArray(writer, "z", value.Z);
        WriteStringArray(writer, "elements", value.Elements);
        if (value.Extra != null)
        {
            foreach ((string key, JsonNode? objValue) in value.Extra)
            {
                if (objValue is null)
                    continue;
                writer.WritePropertyName(key);
                objValue.WriteTo(writer, options);
            }
        }
        writer.WriteEndObject();
    }
    private static void WriteIntArray(Utf8JsonWriter writer, string propertyName, int[] array)
    {
        if (array?.Length is not > 0)
            throw new JsonException($"Property '{propertyName}' cannot be null or empty");
        writer.WritePropertyName(propertyName);
        if (array.Length == 1)
            writer.WriteNumberValue(array[0]);
        else
        {
            writer.WriteStartArray();
            foreach (int value in array)
                writer.WriteNumberValue(value);
            writer.WriteEndArray();
        }
    }
    private static void WriteStringArray(Utf8JsonWriter writer, string propertyName, SortedSet<string> set)
    {
        if (set?.Count is not > 0)
            throw new JsonException($"Property '{propertyName}' cannot be null or empty");
        writer.WritePropertyName(propertyName);
        if (set.Count == 1)
            writer.WriteStringValue(set.Min);
        else
        {
            writer.WriteStartArray();
            foreach (string value in set)
                writer.WriteStringValue(value);
            writer.WriteEndArray();
        }
    }
}