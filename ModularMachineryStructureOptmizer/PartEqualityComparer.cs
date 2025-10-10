using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace ModularMachineryStructureOptmizer;

public class PartEqualityComparer : IEqualityComparer<Machine.Part>
{
    public static readonly PartEqualityComparer Instance = new();

    public bool Equals(Machine.Part? x, Machine.Part? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x == null || y == null)
            return false;
        if (!ElementsEquals(x.Elements, y.Elements))
            return false;
        if (!ExtraEquals(x.Extra, y.Extra))
            return false;
        return true;
    }
    private static bool ElementsEquals(SortedSet<string>? set1, SortedSet<string>? set2)
    {
        if (ReferenceEquals(set1, set2))
            return true;
        if (set1 is null || set2 is null)
            return false;
        if (set1.Count != set2.Count)
            return false;
        return set1.SequenceEqual(set2);
    }
    private static bool ExtraEquals(Dictionary<string, JsonNode>? dict1, Dictionary<string, JsonNode>? dict2)
    {
        if (ReferenceEquals(dict1, dict2))
            return true;
        if (dict1 is null || dict2 is null)
            return false;
        if (dict1.Count != dict2.Count)
            return false;
        foreach (KeyValuePair<string, JsonNode> kvp in dict1)
        {
            if (!dict2.TryGetValue(kvp.Key, out JsonNode? value2))
                return false;
            if (!JsonNodeEquals(kvp.Value, value2))
                return false;
        }
        return true;
    }
    private static bool JsonNodeEquals(JsonNode? node1, JsonNode? node2)
    {
        if (ReferenceEquals(node1, node2))
            return true;
        if (node1 is null || node2 is null)
            return false;
        if (node1.GetType() != node2.GetType())
            return false;
        if (node1 is JsonValue value1 && node2 is JsonValue value2)
        {
            return JsonValueEquals(value1, value2);
        }
        if (node1 is JsonArray array1 && node2 is JsonArray array2)
        {
            if (array1.Count != array2.Count)
                return false;
            for (int i = 0; i < array1.Count; i++)
            {
                if (!JsonNodeEquals(array1[i], array2[i]))
                    return false;
            }
            return true;
        }
        if (node1 is JsonObject obj1 && node2 is JsonObject obj2)
        {
            if (obj1.Count != obj2.Count)
                return false;
            foreach ((string key1, JsonNode? objValue1) in obj1)
            {
                if (!obj2.TryGetPropertyValue(key1, out JsonNode? objValue2))
                    return false;
                if (!JsonNodeEquals(objValue1, objValue2))
                    return false;
            }
            return true;
        }
        return false;
    }
    private static bool JsonValueEquals(JsonValue value1, JsonValue value2)
    {
        if (value1.TryGetValue(out string? str1) && value2.TryGetValue(out string? str2))
            return str1 == str2;
        if (value1.TryGetValue(out long long1) && value2.TryGetValue(out long long2))
            return long1 == long2;
        if (value1.TryGetValue(out double double1) && value2.TryGetValue(out double double2))
            return double1 == double2;
        if (value1.TryGetValue(out bool bool1) && value2.TryGetValue(out bool bool2))
            return bool1 == bool2;
        return value1.ToJsonString() == value2.ToJsonString();
    }
    public int GetHashCode(Machine.Part? obj)
    {
        switch (obj?.Elements?.Count)
        {
            case null:
            case 0:
                return obj?.Extra?.Count ?? 0;
            case 1:
                return HashCode.Combine(obj.Extra?.Count, obj.Elements.Min);
            case 2:
                return HashCode.Combine(obj.Extra?.Count, obj.Elements.Min, obj.Elements.Max);
            default:
                Span<int> hashcodes = stackalloc int[Math.Max(obj.Elements.Count, 7)];
                int i = 0;
                foreach (string? str in obj.Elements)
                {
                    hashcodes[i++] = str?.GetHashCode() ?? 0;
                    if (i == hashcodes.Length)
                        break;
                }
                return hashcodes.Length switch
                {
                    3 => HashCode.Combine(
                        obj.Extra?.Count,
                        hashcodes[0],
                        hashcodes[1],
                        hashcodes[2]),
                    4 => HashCode.Combine(
                        obj.Extra?.Count,
                        hashcodes[0],
                        hashcodes[1],
                        hashcodes[2],
                        hashcodes[3]),
                    5 => HashCode.Combine(
                        obj.Extra?.Count,
                        hashcodes[0],
                        hashcodes[1],
                        hashcodes[2],
                        hashcodes[3],
                        hashcodes[4]),
                    6 => HashCode.Combine(
                        obj.Extra?.Count,
                        hashcodes[0],
                        hashcodes[1],
                        hashcodes[2],
                        hashcodes[3],
                        hashcodes[4],
                        hashcodes[5]),
                    7 => HashCode.Combine(
                        obj.Extra?.Count,
                        hashcodes[0],
                        hashcodes[1],
                        hashcodes[2],
                        hashcodes[3],
                        hashcodes[4],
                        hashcodes[5],
                        hashcodes[6]),
                    _ => HashCode.Combine(obj.Extra?.Count, obj.Elements.Count) // impossible!
                };
        }
    }
}
