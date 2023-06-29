using System;
using UnityEngine;

public class GameUser : MonoBehaviour
{
    IClientGameSocket _socket;
    Battle _battle;

    public bool IsLocalClient { get; internal set; }

    void Start()
    {
        if(Main.Instance.useRealNetwork)
        {
            #if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
            string strIp = RoomUI.ip;
            #else
            string strIp = null;
            #endif
            if(IsLocalClient)
            {
                strIp = "127.0.0.1";
            }
            _socket = new GameClientSocket(strIp);
        }
        else
        {
            _socket = new DumpGameClientSocket();
        }

        _socket.Start();
        _socket.OnStartBattle = OnBattleStart;
    }

    private void OnBattleStart(byte userCount, byte userId)
    {
        _userId = userId;
         _battle = new Battle(Main.Instance.tick, false, false, userId, _socket, userCount);
        // CloseUI

        var roomUI = UnityEngine.Object.FindObjectOfType<RoomUI>();
        if(roomUI)
        {
            roomUI.gameObject.SetActive(false);
        }
    }

    void Update(){
        _battle?.Update();
        _socket.Update();

        if(_socket.connectResult == ConnectResult.Refuse)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        _socket.OnDestroy();
    }

    int _userId = 0;
    void OnGUI()
    {
        if(_battle == null) return;

        GUI.color = Color.black;
        var hash = _battle.CheckSumMgr.CurrentCheckSum;
        GUI.Label(new Rect(0, 50 * _userId, 200, 50), hash.ToString());
    }
}