using System.Collections;
using System.Collections.Immutable;

namespace ModularMachineryStructureOptimizer;

public readonly struct CompressedVoxel<T> where T : notnull, IEquatable<T>, IComparable<T>
{
    public ImmutableSortedSet<T> X { get; }
    public ImmutableSortedSet<T> Y { get; }
    public ImmutableSortedSet<T> Z { get; }
    public int Coverage => checked(X.Count * Y.Count * Z.Count);

    public CompressedVoxel(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z)
    {
        X = [.. x];
        Y = [.. y];
        Z = [.. z];
    }
    public CompressedVoxel(IEnumerable<T> x, IEnumerable<T> y, IEnumerable<T> z)
    {
        X = [.. x];
        Y = [.. y];
        Z = [.. z];
    }

    public IEnumerable<Voxel<T>> Expand()
    {
        foreach (T x in X)
            foreach (T y in Y)
                foreach (T z in Z)
                    yield return new Voxel<T>(x, y, z);
    }
    public List<Voxel<T>> ExpandToList()
    {
        List<Voxel<T>> result = new(Coverage);
        foreach (T x in X)
            foreach (T y in Y)
                foreach (T z in Z)
                    result.Add(new(x, y, z));
        return result;
    }
    public Voxel<T>[] ExpandToArray()
    {
        Voxel<T>[] result = new Voxel<T>[Coverage];
        int i = 0;
        foreach (T x in X)
            foreach (T y in Y)
                foreach (T z in Z)
                    result[i++] = new(x, y, z);
        return result;
    }
}