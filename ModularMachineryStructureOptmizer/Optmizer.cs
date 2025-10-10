namespace ModularMachineryStructureOptmizer;

public class Optmizer
{
    public static IEnumerable<CompressedVoxel<T>> Optmize<T>(IEnumerable<Voxel<T>> voxels)
        where T : notnull, IEquatable<T>, IComparable<T>
    {
        HashSet<Voxel<T>> uncoveredVoxels = [.. voxels];
        HashSet<T> candidateX = [];
        HashSet<T> candidateY = [];
        HashSet<T> candidateZ = [];
        HashSet<T> allXCoords = [];
        HashSet<T> allYCoords = [];
        HashSet<T> allZCoords = [];
        while (uncoveredVoxels.Count != 0)
        {
            CompressedVoxel<T>? bestCandidate = null;
            int maxCoverage = 0;
            foreach (Voxel<T> seedVoxel in uncoveredVoxels)
            {
                CompressedVoxel<T> candidate = ExpandFromSeed(seedVoxel, uncoveredVoxels);
                int coverage = candidate.Coverage;
                if (coverage > maxCoverage)
                {
                    maxCoverage = coverage;
                    bestCandidate = candidate;
                }
            }
            if (bestCandidate == null)
                break;
            CompressedVoxel<T> finalCandidate = bestCandidate.Value;
            foreach (Voxel<T> voxel in finalCandidate.Expand())
                uncoveredVoxels.Remove(voxel);
            yield return finalCandidate;
        }

        CompressedVoxel<T> ExpandFromSeed(Voxel<T> seedVoxel, HashSet<Voxel<T>> allVoxelsSet)
        {
            candidateX.Clear();
            candidateY.Clear();
            candidateZ.Clear();
            candidateX.Add(seedVoxel.X);
            candidateY.Add(seedVoxel.Y);
            candidateZ.Add(seedVoxel.Z);
            while (true)
            {
                (Axis3D axis, T? value, long gain) bestExpansion = (default, default, default);

                allXCoords.Clear();
                allYCoords.Clear();
                allZCoords.Clear();
                foreach (Voxel<T> voxel in allVoxelsSet)
                {
                    allXCoords.Add(voxel.X);
                    allYCoords.Add(voxel.Y);
                    allZCoords.Add(voxel.Z);
                }
                foreach (T xNew in allXCoords)
                {
                    if (candidateX.Contains(xNew))
                        continue;
                    bool isValid = true;
                    foreach (T y in candidateY)
                    {
                        foreach (T z in candidateZ)
                        {
                            if (!allVoxelsSet.Contains(new(xNew, y, z)))
                            {
                                isValid = false;
                                break;
                            }
                        }
                        if (!isValid)
                            break;
                    }
                    if (isValid)
                    {
                        long gain = (long)candidateY.Count * candidateZ.Count;
                        if (gain > bestExpansion.gain)
                            bestExpansion = (Axis3D.X, xNew, gain);
                    }
                }
                foreach (T yNew in allYCoords)
                {
                    if (candidateY.Contains(yNew))
                        continue;
                    bool isValid = true;
                    foreach (T x in candidateX)
                    {
                        foreach (T z in candidateZ)
                        {
                            if (!allVoxelsSet.Contains(new(x, yNew, z)))
                            {
                                isValid = false;
                                break;
                            }
                        }
                        if (!isValid)
                            break;
                    }
                    if (isValid)
                    {
                        long gain = (long)candidateY.Count * candidateZ.Count;
                        if (gain > bestExpansion.gain)
                            bestExpansion = (Axis3D.Y, yNew, gain);
                    }
                }
                foreach (T zNew in allZCoords)
                {
                    if (candidateZ.Contains(zNew))
                        continue;
                    bool isValid = true;
                    foreach (T x in candidateX)
                    {
                        foreach (T y in candidateY)
                        {
                            if (!allVoxelsSet.Contains(new(x, y, zNew)))
                            {
                                isValid = false;
                                break;
                            }
                        }
                        if (!isValid)
                            break;
                    }
                    if (isValid)
                    {
                        long gain = (long)candidateY.Count * candidateZ.Count;
                        if (gain > bestExpansion.gain)
                            bestExpansion = (Axis3D.Z, zNew, gain);
                    }
                }
                if (bestExpansion.gain <= 0)
                    break;
                switch (bestExpansion.axis)
                {
                    case Axis3D.X:
                        candidateX.Add(bestExpansion.value!);
                        break;
                    case Axis3D.Y:
                        candidateY.Add(bestExpansion.value!);
                        break;
                    case Axis3D.Z:
                        candidateZ.Add(bestExpansion.value!);
                        break;
                }
            }
            return new(candidateX, candidateY, candidateZ);
        }
    }
    private enum Axis3D
    {
        X, Y, Z
    }
}