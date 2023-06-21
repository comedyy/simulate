

using System;

public struct fp : IEquatable<fp>, IComparable<fp>
{
    float value;

    public static implicit operator fp(float i)
    {
        return new fp {value=i};
    }

    public static implicit operator float(fp p)
    {
        return p.value;
    }

    public int CompareTo(fp other)
    {
        return value.CompareTo(other);
    }

    public bool Equals(fp other)
    {
        return value == other;
    }
}