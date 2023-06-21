

using System;
using System.Runtime.CompilerServices;

public struct fp : IEquatable<fp>, IComparable<fp>
{
    public static fp zero = fp.Create(0);
    public static fp half = fp.Create(0, 50000);
    public static fp one = fp.Create(1);

    public float rawValue;

    public static fp PositiveInfinity => fp.Create(999999999);

    // 整数部分，小数部分(0-99999);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Create(int integerPart, int fractioncalPart = 0)
    {
        var v = integerPart + fractioncalPart * 0.00001f;
        return new fp(){ rawValue = v };
    }

    public static implicit operator float(fp p)
    {
        return p.rawValue;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator fp(int intValue)
    {
        return new fp(){rawValue = intValue};
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
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp operator /(fp a, fp b) {
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
        return rawValue.ToString();
    }
}