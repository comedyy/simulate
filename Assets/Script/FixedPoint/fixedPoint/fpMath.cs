#if FIXED_POINT
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public class fpMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int asint(fp x)
    {
        return (int)x.rawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp distancesq(fp3 x, fp3 y)
    {
        var d = x - y;
        return d.x * d.x + d.y * d.y + d.z * d.z;
    }

    public static fp PI => fp.Create(3, 1415);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp sin(fp radians)
    {
        radians.rawValue %= fp.pi2.rawValue;
        radians       *= fp.one_div_pi2;
        var raw = fixlut.sin(radians.rawValue);
        fp result;
        result.rawValue = raw;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp cos(fp radians)
    {
        radians.rawValue %= fp.pi2.rawValue;
        radians       *= fp.one_div_pi2;
        return new fp(){rawValue = fixlut.cos(radians.rawValue)};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp3 normalize(fp3 v)
    {
        if (v == fp3.zero)
            return fp3.zero;

        var magnitude = Magnitude(v);
        if(magnitude.rawValue == 0)
        {
            v = v * 1000;
        }

        return v /  Magnitude(v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Magnitude(fp3 v)
    {
        v.x.rawValue =
            ((v.x.rawValue * v.x.rawValue) >> fixlut.PRECISION) +
            ((v.y.rawValue * v.y.rawValue) >> fixlut.PRECISION) +
            ((v.z.rawValue * v.z.rawValue) >> fixlut.PRECISION);
        
        return Sqrt(v.x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Min(fp a, fp b)
    {
        return a < b ? a : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Max(fp a, fp b)
    {
        return a > b ? a : b;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp3 Min(fp3 a, fp3 b)
    {
        return new fp3()
        {
            x = Min(a.x, b.x),
            y = Min(a.y, b.y),
            z = Min(a.z, b.z)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp3 Max(fp3 a, fp3 b)
    {
        return new fp3()
        {
            x = Max(a.x, b.x),
            y = Max(a.y, b.y),
            z = Max(a.z, b.z)
        };
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Abs(fp a)
    {
        return new fp(){rawValue = Unity.Mathematics.math.abs(a.rawValue)};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Sqrt(fp num)
    {
        fp r;

        if (num.rawValue == 0) {
            r.rawValue = 0;
        }
        else {
            var b = (num.rawValue >> 1) + 1L;
            var c = (b + (num.rawValue / b)) >> 1;

            while (c < b) {
                b = c;
                c = (b + (num.rawValue / b)) >> 1;
            }

            r.rawValue = b << (fixlut.PRECISION >> 1);
        }

        if(num.rawValue != 0 && r.rawValue == 0)
        {
            Debug.LogError($"sqrt({num.rawValue}) == zero");
        }

        return r;
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