using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class LogicSyncMain : MonoBehaviour
{
    public int worldCount = 1;
    public int LogicFrameCount = 15;
    public float pingSec = 0;
    public bool usePlaybackInput;
    public bool randomLogicSimulateTime;

    Battle[] _battles;

    // Start is called before the first frame update
    async void Start()
    {
        _battles = new Battle[worldCount];
        for(int i = 0; i < worldCount; i++)
        {
            // await Task.Delay(i * 10);
            _battles[i] = new Battle(fp.Create(0, 660), randomLogicSimulateTime, usePlaybackInput, (i + 1), null, 5);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < _battles.Length; i++)
        {
            if(_battles[i] != null && !_battles[i].IsEnd)
            {
                _battles[i].Update();
            }
        }

        if(_battles.All(m=>m.IsEnd))
        {
            enabled = false;
            CheckCheckSumOfAll("spawn", _battles.Select(m=>m.CheckSumMgr.preRVO).ToArray());
            CheckCheckSumOfAll("pos", _battles.Select(m=>m.CheckSumMgr.positionChecksum).ToArray());
            CheckCheckSumOfAll("hp", _battles.Select(m=>m.CheckSumMgr.hpCheckSum).ToArray());
            CheckCheckSumOfAll("find", _battles.Select(m=>m.CheckSumMgr.targetFindCheckSum).ToArray());
        }
    }

    private void CheckCheckSumOfAll(string name, CheckSum[] _checksums)
    {
        var allCheckSums = _checksums.Select(m=>m.GetHistoryCheckSums()).ToArray();
        Debug.Log($"【{name}】"  +string.Join(",", allCheckSums));
        if(allCheckSums.Distinct().Count() > 1)
        {
            Debug.LogError($"发现不同步 [{name}] checkSumCount:[{_checksums[0].GetHistory().Count}]");

            foreach (var item in _checksums)
            {
                Debug.Log($"【{item.GetHistory().Count}】:" + string.Join(",", item.GetHistory()));
            }

            // 输出不同步的hash input。
            var first = _checksums[0];
            for(int i = 0; i < first.GetHistory().Count; i++)
            {
                for(int j = 1; j < worldCount; j++)
                {
                    // hash 不一样
                    if(first.GetHistory()[i] != _checksums[j].GetHistory()[i])
                    {
                        Debug.LogError($"unEqual frame = {i} targetWorld {j}");
                        Debug.LogError(string.Join(",", first.GetHistoryDetail()[i]));
                        Debug.LogError(string.Join(",", _checksums[j].GetHistoryDetail()[i]));

                        Debug.LogError(string.Join(",", first.GetHistoryDetailOrder()[i]));
                        Debug.LogError(string.Join(",", _checksums[j].GetHistoryDetailOrder()[i]));

                        if(name == "pos")
                        {
                            Debug.LogError(GetPos(first.GetHistoryDetail()[i]));
                            Debug.LogError(GetPos(_checksums[j].GetHistoryDetail()[i]));
                        }
                        return;
                    }
                }
            }
        }
    }

    private string GetPos(List<int> list)
    {
        List<float3> lst = new List<float3>();
        List<float3> lstRotation = new List<float3>();
        for(int i = 0; i < list.Count / 8; i++)
        {
            var f1 = new fp(){rawValue = list[i * 8 + 1]};
            var f2 = new fp(){rawValue = list[i * 8 + 2]};
            var f3 = new fp(){rawValue = list[i * 8 + 3]};
            lst.Add(new float3(f1, f2, f3));

            var f11 = new fp(){rawValue = list[i * 8 + 5]};
            var f21 = new fp(){rawValue = list[i * 8 + 6]};
            var f31 = new fp(){rawValue = list[i * 8 + 7]};
            lstRotation.Add(new float3(f11, f21, f31));
        }

        return $"pos:{string.Join(",", lst)} \n rotation{string.Join(",", lstRotation)}";
    }

    void OnGUI()
    {

    }

    void OnDestroy()
    {
        foreach (var item in _battles)
        {
            item.Dispose();
        }
    }
}
