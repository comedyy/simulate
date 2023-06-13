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

    BattleWorld[] _worlds;
    CheckSum[] _checksums; 
    // Start is called before the first frame update
    async void Start()
    {
        _worlds = new BattleWorld[worldCount];
        _checksums = new CheckSum[worldCount];
        for(int i = 0; i < worldCount; i++)
        {
            _checksums[i] = new CheckSum();
        }

        for(int i = 0; i < worldCount; i++)
        {
            await Task.Delay(i * 10);
            _worlds[i] = new BattleWorld("new " + i, _checksums[i]);
            Debug.Log(Time.time);
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
            CheckCheckSumOfAll(_checksums);
        }
    }

    private void CheckCheckSumOfAll(CheckSum[] _checksums)
    {
        var allCheckSums = _checksums.Select(m=>m.GetHistoryCheckSums()).ToArray();
        Debug.Log(string.Join(",", allCheckSums));
        if(allCheckSums.Distinct().Count() > 1)
        {
            Debug.LogError("发现不同步");

            foreach (var item in _checksums)
            {
                Debug.Log(string.Join(",", item.GetHistory()));
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
