using ModularMachineryStructureOptimizer.JsonConverters;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModularMachineryStructureOptimizer;

[JsonConverter(typeof(MachineJsonConverter))]
public class Machine
{
    public List<Part>? Parts { get; set; }
    public List<DynamicPattern>? DynamicPatterns { get; set; }
    public Dictionary<string, JsonNode>? Extra { get; set; }


    [JsonConverter(typeof(PartJsonConverter))]
    public class Part
    {
        public required int[] X { get; set; }
        public required int[] Y { get; set; }
        public required int[] Z { get; set; }
        public required SortedSet<string> Elements { get; set; }
        public Dictionary<string, JsonNode>? Extra { get; set; }
        public int Coverage => checked(X.Length * Y.Length * Z.Length);
        public bool IsValidPart => X?.Length is > 0 && Y?.Length is > 0 && Z?.Length is > 0 && Elements?.Count is > 0;
        public CompressedVoxel<int> ToCompressedVoxel() => new(X, Y, Z.AsSpan());
        public Part WithVoxel(Voxel<int> voxel) => new()
        {
            X = [voxel.X],
            Y = [voxel.Y],
            Z = [voxel.Z],
            Elements = Elements,
            Extra = Extra
        };
        public Part WithVoxel(CompressedVoxel<int> voxel) => new()
        {
            X = [.. voxel.X],
            Y = [.. voxel.Y],
            Z = [.. voxel.Z],
            Elements = Elements,
            Extra = Extra
        };
        public IEnumerable<Voxel<int>> Expand()
        {
            foreach (int x in X)
                foreach (int y in Y)
                    foreach (int z in Z)
                        yield return new Voxel<int>(x, y, z);
        }
        public override string ToString()
        {
            return $"<{Elements?.FirstOrDefault((string?)null) ?? "!"}{(Elements?.Count is > 1 ? $" and {Elements.Count - 1} more" : "")}>";
        }
    }
    [JsonConverter(typeof(DynamicPatternJsonConverter))]
    public class DynamicPattern
    {
        public List<Part>? Parts { get; set; }
        public List<Part>? PartsEnd { get; set; }
        public Dictionary<string, JsonNode>? Extra { get; set; }
    }
}