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
    public int countUser = 1;
    public float pingSec = 0;
    public bool savePlayback;
    public bool useRealNetwork;

    Server _dumpServer;
    public fp tick => fp.Create(0, 660);
    // Battle[] _battles;

    public static Main Instance{get; private set;}

    // Start is called before the first frame update
    async void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        Instance = this;
        // var tick = 1f / LogicFrameCount;
        // DumpGameServerSocket socket = new DumpGameServerSocket(pingSec);
        // _dumpServer = new Server(tick, socket);

        // _battles = new Battle[countUser];
        // for(int i = 0; i < countUser; i++)
        // {
        //     var clientSocket = new DumpGameClientSocket(_dumpServer.ServerDumpSocket);
        //     _battles[i] = new Battle(tick, pingSec, false, false, i + 1, clientSocket, countUser);
        // }
    }

    public void StartBattle(Server server)
    {
        _dumpServer = server;

        // var tick = 1f / LogicFrameCount;
        // _battles = new Battle[listClient.Count];
        // for(int i = 0; i < _battles.Length; i++)
        // {
        //     _battles[i] = new Battle(tick, pingSec, false, false, i + 1, listClient[i], countUser);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // foreach(var battle in _battles)
        // {
        //     if(battle != null && !battle.IsEnd)
        //     {
        //         battle.Update(pingSec);
        //     }
        // }
        
        _dumpServer?.Update();
    }

    void OnDestroy()
    {
        // foreach (var item in _battles)
        // {
        //     item.Dispose();
        // }
    }
}
