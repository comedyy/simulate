
#if FIXED_POINT

using System;
using System.Runtime.CompilerServices;

public struct fp : IEquatable<fp>, IComparable<fp>
{
    public const int count = 16;
    public static fp zero = fp.Create(0);
    public static fp half = fp.Create(0, 5000);
    public static fp _0_25 = fp.Create(0, 2500);
    public static fp one = fp.Create(1);

    public long rawValue;
    public static readonly fp pi          = new fp(){rawValue = 205887L};
    public static readonly fp pi2         = pi * 2;
    public static readonly fp one_div_pi2 = 1 / pi2;

    public static fp PositiveInfinity => fp.Create(999999999);

    public static fp EPSILON => new fp(){rawValue = 1};

    // 整数部分，小数部分(0-9999);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Create(int integerPart, int fractioncalPart = 0)
    {
        if(integerPart != 0 && fractioncalPart < 0) throw new Exception($"创建错误 {integerPart} {fractioncalPart}");

        long fraction = fractioncalPart;
        fraction = (fraction * 65536 / 10000);

        if(integerPart >= 0)
        {
            var v = (integerPart << count) + fraction;
            return new fp(){ rawValue = v };    
        }
        else
        {
            var v = (integerPart << count) - fraction;
            return new fp(){ rawValue = v };    
        }
    }

    public static implicit operator float(fp p)
    {
        return (float)p.rawValue / (1 << count);
    }
    
    public static fp UnsafeConvert(float f)
    {
        return new fp(){
            rawValue = (long)(f * one.rawValue)
        };
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator fp(int intValue)
    {
        return new fp(){rawValue = intValue << count};
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp operator -(fp a) {
        a.rawValue = -a.rawValue;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp operator +(fp a, fp b) {
        a.rawValue += b.rawValue;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp operator -(fp a, fp b) {
        a.rawValue -= b.rawValue;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp operator *(fp a, fp b) {
        a.rawValue *= b.rawValue;
        a.rawValue >>= count;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp operator /(fp a, fp b) {
        a.rawValue <<= count;
        if(b.rawValue == 0)
        {
            throw new Exception($"{a} / 0");
        }
        a.rawValue /= b.rawValue;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(fp a, fp b) {
        return a.rawValue < b.rawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(fp a, fp b) {
        return a.rawValue <= b.rawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(fp a, fp b) {
        return a.rawValue > b.rawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(fp a, fp b) {
        return a.rawValue >= b.rawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(fp a, fp b) {
        return a.rawValue == b.rawValue;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(fp a, fp b) {
        return a.rawValue != b.rawValue;
    }

    public int CompareTo(fp other)
    {
        return rawValue.CompareTo(other);
    }

    public bool Equals(fp other)
    {
        return rawValue == other.rawValue;
    }

    public override bool Equals(object obj)
    {
        return obj is fp other && this == other;
    }

    public override int GetHashCode()
    {
        return rawValue.GetHashCode();
    }

    public override string ToString()
    {
        return $"raw:【{rawValue}】float:【{(float)this}】";
    }
}
#endif
