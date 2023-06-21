
public static class RandomUtils
{
    public static fp Random( System.Random _random)
    {
        return new fp(){rawValue = _random.Next(0, 10000) / 10000f};
    }

    public static fp Random(System.Random _random, fp random)
    {
        return random * Random(_random);
    }
}
