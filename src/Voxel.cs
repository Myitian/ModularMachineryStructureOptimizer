namespace ModularMachineryStructureOptimizer;

public struct Voxel<T>(T x, T y, T z) : IEquatable<Voxel<T>>, IComparable<Voxel<T>> where T : notnull, IEquatable<T>, IComparable<T>
{
    public T X { get; set; } = x;
    public T Y { get; set; } = y;
    public T Z { get; set; } = z;

    public readonly int CompareTo(Voxel<T> other) =>
        X.CompareTo(other.X) is int cmpX and not 0 ? cmpX :
        Y.CompareTo(other.Y) is int cmpY and not 0 ? cmpY :
        Z.CompareTo(other.Z);
    public readonly bool Equals(Voxel<T> other)
        => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    public override readonly bool Equals(object? obj)
        => obj is Voxel<T> voxel && Equals(voxel);
    public override readonly int GetHashCode()
        => HashCode.Combine(X, Y, Z);
    public override readonly string ToString()
        => $"<{X},{Y},{Z}>";
    public static bool operator ==(Voxel<T> left, Voxel<T> right)
        => left.Equals(right);
    public static bool operator !=(Voxel<T> left, Voxel<T> right)
        => !left.Equals(right);
    public static bool operator <(Voxel<T> left, Voxel<T> right)
        => left.CompareTo(right) < 0;
    public static bool operator <=(Voxel<T> left, Voxel<T> right)
        => left.CompareTo(right) <= 0;
    public static bool operator >(Voxel<T> left, Voxel<T> right)
        => left.CompareTo(right) > 0;
    public static bool operator >=(Voxel<T> left, Voxel<T> right)
        => left.CompareTo(right) >= 0;
}