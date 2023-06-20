using System;
using UnityEngine;

public class GameUser : MonoBehaviour
{
    IClientGameSocket _socket;
    Battle _battle;
    void Start()
    {
        if(Main.Instance.useRealNetwork)
        {
            _socket = new GameClientSocket();
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
}