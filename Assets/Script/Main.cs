﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Main : MonoBehaviour
{
    BattleWorld[] _worlds = new BattleWorld[10];
    CheckSum[] _checksums = new CheckSum[10];
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            _checksums[i] = new CheckSum();
            _worlds[i] = new BattleWorld("new " + i, _checksums[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < _worlds.Length; i++)
        {
            if(!_worlds[i].IsEnd)
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
        foreach(var x in _checksums)
        {
            Debug.LogError(x.GetHistoryCheckSums());
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
