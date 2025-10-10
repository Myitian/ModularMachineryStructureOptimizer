using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace ModularMachineryStructureOptmizer;

class Program
{
    static void Main()
    {
        Console.WriteLine("Input:");
        FileInfo fileInput = new(Console.ReadLine().AsSpan().Trim().Trim('"').ToString());
        if (!fileInput.Exists)
        {
            Console.WriteLine("File not exists!");
            return;
        }
        Machine? machine = null;
        using (FileStream fs = fileInput.OpenRead())
        {
            try
            {
                machine = JsonSerializer.Deserialize<Machine>(fs);
            }
            catch
            {
            }
        }
        if (machine is null)
        {
            Console.WriteLine("Invalid machine!");
            return;
        }
        OptmizePartList(machine.Parts);
        if (machine.DynamicPatterns?.Count is > 0)
        {
            foreach (Machine.DynamicPattern pattern in machine.DynamicPatterns)
            {
                OptmizePartList(pattern.Parts);
                OptmizePartList(pattern.PartsEnd);
            }
        }
        Console.WriteLine("Output:");
        FileInfo fileOutput = new(Console.ReadLine().AsSpan().Trim().Trim('"').ToString());
        using (FileStream fs = fileOutput.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            JsonSerializer.Serialize(fs, machine);
        Console.WriteLine("Done!");
    }

    static void OptmizePartList(List<Machine.Part>? parts)
    {
        if (parts is null)
            return;
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
        parts.Clear();
        foreach ((Machine.Part part, List<Voxel<int>> voxels) in materialToVoxels)
            foreach (CompressedVoxel<int> cv in Optmizer.Optmize(voxels))
                parts.Add(part.WithVoxel(cv));
    }
}
