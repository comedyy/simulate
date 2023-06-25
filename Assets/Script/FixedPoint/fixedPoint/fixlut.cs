public static partial class fixlut {
    public const int  FRACTIONS_COUNT = 5;
    public const int  PRECISION       = 16;
    public const int  SHIFT           = 16 - 9;
    public const long PI              = 205887L;
    public const long ONE             = 1 << PRECISION;
    public const long HALF            = 1 << (PRECISION-1);
    public const long ZERO            = 0;
    

    public static long sin(long value) {
        var sign = 1;
        if (value < 0) {
            value = -value;
            sign = -1;
        }

        var index    = (int) (value >> SHIFT);
        var fraction = (value - (index << SHIFT)) << 9;
        var a        = SinLut[index];
        var b        = SinLut[index + 1];
        var v2       = a + (((b - a) * fraction) >> PRECISION);
        return v2 * sign;
    }

    public static long cos(long value) {
        if (value < 0) {
            value = -value;
        }

        value += fp._0_25.rawValue;
        
        if (value >= 65536) {
            value -= 65536;
        }

        var index    = (int) (value >> SHIFT);
        var fraction = (value - (index << SHIFT)) << 9;
        var a        = SinLut[index];
        var b        = SinLut[index + 1];
        var v2       = a + (((b - a) * fraction) >> PRECISION);
        return v2;
    }
}