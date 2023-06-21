using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public struct fp3 : System.IEquatable<fp3>, IFormattable
{
    public fp x;
    public fp y;
    public fp z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public fp3(fp x, fp y, fp z)
    { 
        this.x = x;
        this.y = y;
        this.z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(fp3 rhs) { return x == rhs.x && y == rhs.y && z == rhs.z; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return string.Format("fp3({0}f, {1}f, {2}f)", x.ToString(), y.ToString(), z.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp3 operator -(fp3 a, fp3 b) {
        a.x.rawValue -= b.x.rawValue;
        a.y.rawValue -= b.y.rawValue;
        a.z.rawValue -= b.z.rawValue;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp3 operator +(fp3 a, fp3 b) {
        a.x.rawValue += b.x.rawValue;
        a.y.rawValue += b.y.rawValue;
        a.z.rawValue += b.z.rawValue;
        return a;
    }

    public static implicit operator Vector3(fp3 p)
    {
        return new Vector3(p.x, p.y, p.z);
    }

    public static implicit operator float3(fp3 p)
    {
        return new float3(p.x, p.y, p.z);
    }

    public override int GetHashCode()
    {
        return (int)fpMath.hash(this);
    }
}