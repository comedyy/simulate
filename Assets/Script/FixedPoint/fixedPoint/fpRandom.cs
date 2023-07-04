using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct fpRandom
{
    public uint state;

    /// <summary>
    /// Constructs a Random instance with a given seed value. The seed must be non-zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public fpRandom(uint seed)
    {
        state = seed;
        CheckInitState();
        NextState();
    }

    /// <summary>
    /// Initialized the state of the Random instance with a given seed value. The seed must be non-zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InitState(uint seed = 0x6E624EB7u)
    {
        state = seed;
        NextState();
    }

    /// <summary>Returns a uniformly random int value in the interval [-2147483647, 2147483647].</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt()
    {
        return (int)NextState() ^ -2147483648;
    }

    /// <summary>Returns a uniformly random int value in the interval [0, max).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt(int max)
    {
        CheckNextIntMax(max);
        return (int)((NextState() * (ulong)max) >> 32);
    }

    /// <summary>Returns a uniformly random int value in the interval [min, max).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt(int min, int max)
    {
        CheckNextIntMinMax(min, max);
        uint range = (uint)(max - min);
        return (int)(NextState() * (ulong)range >> 32) + min;
    }

    public fp NextFp(fp min, fp max)
    {
        uint range = (uint)(max.rawValue - min.rawValue);
        var nextInt = (int)(NextState() * (ulong)range >> 32) + min.rawValue;
        return new fp(){rawValue = nextInt};
    }
/// --------------------- private -----------------------



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint NextState()
    {
        CheckState();
        uint t = state;
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return t;
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private void CheckState()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if(state == 0)
            throw new System.ArgumentException("Invalid state 0. Random object has not been properly initialized");
#endif
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private void CheckInitState()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (state == 0)
            throw new System.ArgumentException("Seed must be non-zero");
#endif
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private void CheckNextIntMinMax(int min, int max)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (min > max)
            throw new System.ArgumentException("min must be less than or equal to max");
#endif
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private void CheckNextIntMax(int max)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (max < 0)
            throw new System.ArgumentException("max must be positive");
#endif
    }
}