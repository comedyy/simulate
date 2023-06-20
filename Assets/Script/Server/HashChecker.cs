using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

struct NotSameStruct
{
    public string name;
    public int index;
}

public class HashChecker
{
    List<FrameHash>[] _listHash;
    int checkedLength = 0;
    int notSamePosIndex{get; set;} = -1;
    int notSameHpIndex{get; set;} = -1;
    int notSameFindTargetIndex{get; set;} = -1;
    public bool NotSame => notSamePosIndex >= 0 || notSameHpIndex >= 0 || notSameFindTargetIndex >= 0;

    public HashChecker(int maxCount)
    {
        _listHash = new List<FrameHash>[maxCount];
        for(int i = 0; i < _listHash.Length; i++)
        {
            _listHash[i] = new List<FrameHash>();
        }
    }

    public void AddHash(FrameHash hash)
    {
        var id = hash.id;
        var list = _listHash[id - 1];
        list.Add(hash);

        CheckHash();
    }

    private void CheckHash()
    {
        var minCount = _listHash.Min(m=>m.Count);
        if(minCount > checkedLength)
        {
            for(int i = checkedLength + 1; i <= minCount; i++)
            {
                CheckIndex(i - 1);
            }

            checkedLength = minCount;
        }
    }

    private void CheckIndex(int v)
    {
        List<FrameHash> list = new List<FrameHash>();
        foreach(var x in _listHash)
        {
            list.Add(x[v]);
        }

        var first = list[0];

        var positionHashSame = list.All(m=>m.hashPos == first.hashPos);
        if(!positionHashSame && notSamePosIndex < 0)
        {
            notSamePosIndex = v;
        }

        var hpHashSame = list.All(m=>m.hashHp == first.hashHp);
        if(!hpHashSame && notSameHpIndex < 0)
        {
            notSameHpIndex = v;
        }

        var targetHashSame = list.All(m=>m.hashFindtarget == first.hashFindtarget);
        if(!targetHashSame && notSameFindTargetIndex < 0)
        {
            notSameFindTargetIndex = v;
        }

        if(NotSame)
        {
            Debug.LogError($"发现不一致的问题, pos：{notSamePosIndex} hp:{notSameHpIndex} targetFind {notSameFindTargetIndex}");
            if(notSamePosIndex >= 0) EchoPosIndex(list.Select(m=>m.hashPos), "Pos");
            if(notSameHpIndex >= 0) EchoIndex(list.Select(m=>m.hashHp), "Hp");
            if(notSameFindTargetIndex >= 0) EchoIndex(list.Select(m=>m.hashFindtarget), "FindTarget");
        }
    }

    private void EchoIndex(IEnumerable<FrameHashItem> enumerable, string v)
    {
        foreach (var x in enumerable)
        {
            Debug.LogError($"{v} {x.ToString()}");
        }
    }

    
    private void EchoPosIndex(IEnumerable<FrameHashItem> enumerable, string v)
    {
        foreach (var x in enumerable)
        {
            Debug.LogError($"{v} {GenPosHashDetail(x)}");
        }
    }

    string GenPosHashDetail(FrameHashItem item)
    {
        List<float3> lst = new List<float3>();
        List<float4> lstRotation = new List<float4>();
        for(int i = 0; i < item.listEntity.Count / 2; i++)
        {
            var f1 = math.asfloat(item.listValue[i * 9 + 1]);
            var f2 = math.asfloat(item.listValue[i * 9 + 2]);
            var f3 = math.asfloat(item.listValue[i * 9 + 3]);
            lst.Add(new float3(f1, f2, f3));

            var f11 = math.asfloat(item.listValue[i * 9 + 5]);
            var f21 = math.asfloat(item.listValue[i * 9 + 6]);
            var f31 = math.asfloat(item.listValue[i * 9 + 7]);
            var f41 = math.asfloat(item.listValue[i * 9 + 8]);
            lstRotation.Add(new float4(f11, f21, f31, f41));
        }


        return $"Hash:{item.hash} \nlistValue：{string.Join("!", item.listValue)}\n listEntity：{string.Join("!", item.listEntity)}\n pos:{string.Join(",", lst)} \n rotation{string.Join(",", lstRotation)}";
    }
}