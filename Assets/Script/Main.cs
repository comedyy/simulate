using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Main : MonoBehaviour
{
    public int worldCount = 1;
    public int LogicFrameCount = 20;
    public float pingSec = 0;

    BattleWorld[] _worlds;
    CheckSumMgr[] _checksums; 
    LocalFrame _localFrame;
    DumpServer _dumpServer;
    DumpNetworkTransferLayer _transLayer;

    // Start is called before the first frame update
    async void Start()
    {
        var tick = 1f / LogicFrameCount;

        _dumpServer = new DumpServer();
        _localFrame = new LocalFrame();
        _transLayer = new DumpNetworkTransferLayer(pingSec);

        _localFrame.Init(tick, _transLayer.Send);
        _dumpServer.Init(tick, _transLayer.Receive);

        _transLayer.Init(_dumpServer.AddMessage, _localFrame.OnReceive);


        _worlds = new BattleWorld[worldCount];
        _checksums = new CheckSumMgr[worldCount];
        for(int i = 0; i < worldCount; i++)
        {
            _checksums[i] = new CheckSumMgr();
        }

        for(int i = 0; i < worldCount; i++)
        {
            await Task.Delay(i * 10);
            _worlds[i] = new BattleWorld("new " + i, _checksums[i], tick, _localFrame, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < _worlds.Length; i++)
        {
            if(_worlds[i] != null && !_worlds[i].IsEnd)
            {
                _worlds[i].Update();
            }
        }

        if(_worlds.All(m=>m.IsEnd))
        {
            enabled = false;
            CheckCheckSumOfAll("pos", _checksums.Select(m=>m.positionChecksum).ToArray());
            CheckCheckSumOfAll("hp", _checksums.Select(m=>m.hpCheckSum).ToArray());
            CheckCheckSumOfAll("find", _checksums.Select(m=>m.targetFindCheckSum).ToArray());
        }

        _transLayer.Update(pingSec);
        _localFrame.Update();
        _dumpServer.Update();
    }

    private void CheckCheckSumOfAll(string name, CheckSum[] _checksums)
    {
        var allCheckSums = _checksums.Select(m=>m.GetHistoryCheckSums()).ToArray();
        Debug.Log($"【{name}】"  +string.Join(",", allCheckSums));
        if(allCheckSums.Distinct().Count() > 1)
        {
            Debug.LogError("发现不同步");

            foreach (var item in _checksums)
            {
                Debug.Log($"【{item.GetHistory().Count}】:" + string.Join(",", item.GetHistory()));
            }

            // 输出不同步的hash input。
            var first = _checksums[0];
            for(int i = 0; i < first.GetHistory().Count; i++)
            {
                for(int j = 1; j < _worlds.Length; j++)
                {
                    // hash 不一样
                    if(first.GetHistory()[i] != _checksums[j].GetHistory()[i])
                    {
                        Debug.LogError($"unEqual frame = {i}");
                        Debug.LogError(string.Join(",", first.GetHistoryDetail()[i]));
                        Debug.LogError(string.Join(",", _checksums[j].GetHistoryDetail()[i]));

                        Debug.LogError(string.Join(",", first.GetHistoryDetailOrder()[i]));
                        Debug.LogError(string.Join(",", _checksums[j].GetHistoryDetailOrder()[i]));
                        return;
                    }
                }
            }
        }
    }

    void OnGUI()
    {

    }

    void OnDestroy()
    {
        foreach (var item in _worlds)
        {
            item.Dispose();
        }
    }
}
