#if !FIXED_POINT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

[StructLayout(LayoutKind.Explicit)]
internal struct IntFloatUnion
{
    [FieldOffset(0)]
    public int intValue;
    [FieldOffset(0)]
    public float floatValue;
}

public class fpMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int asint(fp x)
    {
        IntFloatUnion u;
        u.intValue = 0;
        u.floatValue = x.rawValue;
        return u.intValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp distancesq(fp3 x, fp3 y)
    {
        var d = x - y;
        return d.x * d.x + d.y * d.y + d.z * d.z;
    }

    public static fp PI => new fp(){rawValue = 3.1415f};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp sin(fp radians)
    {
        return new fp(){
            rawValue = Unity.Mathematics.math.sin(radians)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp cos(fp radians)
    {
        return new fp(){
            rawValue = Unity.Mathematics.math.cos(radians)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp3 normalize(fp3 x)
    {
        Unity.Mathematics.float3 xx = new Unity.Mathematics.float3(x.x, x.y, x.z);
        xx = Unity.Mathematics.math.normalize(xx);
        return new fp3(new fp(){rawValue = xx.x}, new fp(){rawValue = xx.y}, new fp(){rawValue = xx.z});
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Min(fp a, fp b)
    {
        return new fp(){rawValue = Unity.Mathematics.math.min(a, b)};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Max(fp a, fp b)
    {
        return new fp(){rawValue = Unity.Mathematics.math.max(a, b)};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Abs(fp a)
    {
        return new fp(){rawValue = Unity.Mathematics.math.abs(a)};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Sqrt(fp scalar)
    {
        return new fp(){rawValue = Unity.Mathematics.math.sqrt(scalar)};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint hash(fp3 v)
    {
        return csum(asuint(v) * new Unity.Mathematics.uint3(0x9B13B92Du, 0x4ABF0813u, 0x86068063u)) + 0xD75513F9u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint3 asuint(fp3 x) { return new Unity.Mathematics.uint3((uint)asint(x.x), (uint)asint(x.y), (uint)asint(x.z)); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint csum(uint3 x) { return x.x + x.y + x.z; }
}

#endif