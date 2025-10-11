using ModularMachineryStructureOptimizer;
using ModularMachineryStructureOptimizer.Arguments;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

const string ArgHelp = "--help";
const string ArgInput = "--input";
const string ArgOutput = "--output";
const string ArgMode = "--mode";

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
ArgParser argx = new(args,
    new(ArgHelp, 0, "-h", "-?"),
    new(ArgInput, 1, "-in", "-i") { Info = "string: Input JSON file path" },
    new(ArgOutput, 1, "-out", "-o") { Info = "string: Output JSON file path" },
    new(ArgMode, 1, "-mode", "-m") { Info = "enum: Mode, accepted={optimize,expand}" }
);
if (argx.Results.ContainsKey(ArgHelp))
{
    argx.WriteHelp(Console.Error);
    Console.Error.WriteLine("""

                Optimize mode: Uses compressed part format to reduce file size.
                Expand mode: Expands all compressed part format to normal format.
                """);
    return 0;
}
if (!argx.TryGetEnum(ArgMode, out Mode mode))
    mode = Mode.Optimize;
JsonSerializerOptions options = new()
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
Machine? machine = null;
using (Stream input = OpenInputStream(argx))
{
    try
    {
        machine = JsonSerializer.Deserialize<Machine>(input, options);
    }
    catch
    {
    }
}
if (machine is null)
{
    Console.Error.WriteLine("Invalid machine!");
    return 1;
}
switch (mode)
{
    case Mode.Optimize:
        OptimizePartList(machine.Parts);
        if (machine.DynamicPatterns?.Count is > 0)
        {
            foreach (Machine.DynamicPattern pattern in machine.DynamicPatterns)
            {
                OptimizePartList(pattern.Parts);
                OptimizePartList(pattern.PartsEnd);
            }
        }
        break;
    case Mode.Expand:
        ExpandPartList(machine.Parts);
        if (machine.DynamicPatterns?.Count is > 0)
        {
            foreach (Machine.DynamicPattern pattern in machine.DynamicPatterns)
            {
                ExpandPartList(pattern.Parts);
                ExpandPartList(pattern.PartsEnd);
            }
        }
        break;
    default:
        Console.Error.WriteLine("Invalid mode!");
        return 1;
}
using (Stream output = OpenOutputStream(argx))
    JsonSerializer.Serialize(output, machine, options);
Console.Error.WriteLine("Done!");
return 0;

static Stream OpenInputStream(ArgParser argx)
{
    if (!argx.TryGetString(ArgInput, out string? path))
    {
        Console.Error.WriteLine("Input:");
        path = Console.In.ReadLine().AsSpan().Trim().Trim('"').ToString();
    }
    if (path == "")
        return Console.OpenStandardInput();
    return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
}
static Stream OpenOutputStream(ArgParser argx)
{
    if (!argx.TryGetString(ArgOutput, out string? path))
    {
        Console.Error.WriteLine("Output:");
        path = Console.In.ReadLine().AsSpan().Trim().Trim('"').ToString();
    }
    if (path == "")
        return Console.OpenStandardOutput();
    return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
}
static void OptimizePartList(List<Machine.Part>? parts)
{
    if (parts is null)
        return;
    Dictionary<Machine.Part, List<Voxel<int>>> materialToVoxels = LoadVoxelsByMaterials(parts);
    parts.Clear();
    foreach ((Machine.Part part, List<Voxel<int>> voxels) in materialToVoxels)
        foreach (CompressedVoxel<int> cv in Optimizer.Optimize(voxels))
            parts.Add(part.WithVoxel(cv));
}
static void ExpandPartList(List<Machine.Part>? parts)
{
    if (parts is null)
        return;
    Dictionary<Machine.Part, List<Voxel<int>>> materialToVoxels = LoadVoxelsByMaterials(parts);
    parts.Clear();
    foreach ((Machine.Part part, List<Voxel<int>> voxels) in materialToVoxels)
    {
        voxels.Sort();
        foreach (Voxel<int> voxel in voxels)
            parts.Add(part.WithVoxel(voxel));
    }
}
static Dictionary<Machine.Part, List<Voxel<int>>> LoadVoxelsByMaterials(List<Machine.Part> parts)
{
    Dictionary<Machine.Part, List<Voxel<int>>> materialToVoxels = new(PartEqualityComparer.Instance);
    foreach (Machine.Part part in parts)
    {
        if (part?.IsValidPart is not true)
            continue;
        if (materialToVoxels.TryGetValue(part, out List<Voxel<int>>? voxels))
            voxels.EnsureCapacity(voxels.Count + part.Coverage);
        else
            materialToVoxels.Add(part, voxels = new(part.Coverage));
        voxels.AddRange(part.Expand());
    }
    return materialToVoxels;
}
enum Mode
{
    Optimize,
    Expand
}