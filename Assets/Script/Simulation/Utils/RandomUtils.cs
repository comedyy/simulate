
public static class RandomUtils
{
    public static fp Random( System.Random _random)
    {
        #if FIXED_POINT
        int max = (int)fp.one.rawValue;
        return new fp(){rawValue = _random.Next(0, max)};
        #else
        return fp.Create(0, _random.Next(0, 10000));
        #endif
    }

    public static fp Random(System.Random _random, fp random)
    {
        return random * Random(_random);
    }

    public static fp Random(System.Random _random, fp from, fp to)
    {
        return (to - from) * Random(_random) + from;
    }
}
