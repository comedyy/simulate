
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Entities;


public class CheckSumComponet : IComponentData
{
    public CheckSumMgr checkSum;
}

public class CheckSum
{
    string name;
    int m_Frame;
    int m_HashCode;
    List<int> listDetail = new List<int>();
    List<Entity> listOrder = new List<Entity>();

    List<int> m_HistoryCheckSums = new List<int>();
    List<List<int>> m_HistoryCheckSumsDetail = new List<List<int>>();
    List<List<Entity>> m_HistoryCheckSumsOrder = new List<List<Entity>>();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy()
    {
        m_HistoryCheckSums?.Clear();
        m_HistoryCheckSumsDetail.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CombineHashCode(int seed, int hashcode)
    {
        seed ^= unchecked(hashcode + (int)0x9e3779b9 + (seed << 6) + (seed >> 2));
        return seed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckValue(Entity entity, int value, params int[] list)
    {
        listDetail.Add(value);
        listDetail.AddRange(list);
        listOrder.Add(entity);

        m_HashCode = CombineHashCode(m_HashCode, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCheckSum() => m_HashCode;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveCheckSum(){
        m_HistoryCheckSums.Add(m_HashCode);
        m_HistoryCheckSumsDetail.Add(listDetail);
        m_HistoryCheckSumsOrder.Add(listOrder);

        m_HashCode = 0;
        listDetail = new List<int>();
        listOrder = new List<Entity>();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<int> GetHistory() => m_HistoryCheckSums;
    public List<List<int>> GetHistoryDetail() => m_HistoryCheckSumsDetail;
    public List<List<Entity>> GetHistoryDetailOrder() => m_HistoryCheckSumsOrder;

    public int GetHistoryCheckSums() => m_HistoryCheckSums.Count > 0 ? m_HistoryCheckSums.Last() : 0;
}