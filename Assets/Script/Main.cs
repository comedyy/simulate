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
    public int LogicFrameCount = 15;
    public float pingSec = 0;
    public bool savePlayback;

    Battle _battle;

    // Start is called before the first frame update
    async void Start()
    {
        var tick = 1f / LogicFrameCount;
        _battle = new Battle(tick, pingSec, false, false, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if(_battle != null && !_battle.IsEnd)
        {
            _battle.Update(pingSec);
        }
    }

    void OnDestroy()
    {
        _battle.Dispose();
    }
}
