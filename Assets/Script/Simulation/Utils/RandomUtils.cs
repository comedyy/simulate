
public static class RandomUtils
{
    public static float Random( System.Random _random)
    {
        return _random.Next(0, 10000) / 10000f;
    }

    public static float Random(System.Random _random, float random)
    {
        return random * Random(_random);
    }
}
