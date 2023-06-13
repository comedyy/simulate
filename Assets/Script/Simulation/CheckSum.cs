
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class CheckSum
{
    int m_Frame;
    int m_HashCode;
    List<int> m_HistoryCheckSums = new List<int>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(int frame)
    {
        m_Frame = frame;
        m_HashCode = CombineHashCode(0, frame);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy()
    {
        m_HistoryCheckSums?.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CombineHashCode(int seed, int hashcode)
    {
        seed ^= unchecked(hashcode + (int)0x9e3779b9 + (seed << 6) + (seed >> 2));
        return seed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckValue(int value)
    {
        m_HashCode = CombineHashCode(m_HashCode, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCheckSum() => m_HashCode;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveCheckSum() => m_HistoryCheckSums.Add(m_HashCode);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<int> GetHistory() => m_HistoryCheckSums;

    public int GetHistoryCheckSums()
    {
        int checksum = 0;
        foreach (var item in m_HistoryCheckSums)
        {
            checksum = CombineHashCode(checksum, item);
        }

        return checksum;
    }
}