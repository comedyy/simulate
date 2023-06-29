using System;
using UnityEngine;

public class GameUser : MonoBehaviour
{
    IClientGameSocket _socket;
    Battle _battle;

    bool IsLocalClient { get; set; }

    private bool userAutoMove;

    void Start()
    {
        if(Main.Instance.useRealNetwork)
        {
            string strIp = RoomUI.ip;
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
         _battle = new Battle(Main.Instance.tick, false, false, userId, _socket, userCount, userAutoMove);
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
        GUI.Label(new Rect(0, 50 * _userId, 100, 50), hash.ToString());
        GUI.Label(new Rect(100 , 50 * _userId, 100, 50), $"cout:{MoveByDirSystem.Count}");
    }

    internal void Init(bool isLocalClient, bool userAutoMove)
    {
        this.IsLocalClient = isLocalClient;
        this.userAutoMove = userAutoMove;
    }
}