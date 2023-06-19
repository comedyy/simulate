using System;
using UnityEngine;

public class GameUser : MonoBehaviour
{
    public static int count;
    DumpGameClientSocket _socket;
    Battle _battle;
    void Start()
    {
        _socket = new DumpGameClientSocket();
        if(!_socket.Connect())
        {
            Destroy(gameObject);
            return;
        }

        _socket.OnStartBattle = OnBattleStart;
    }

    private void OnBattleStart(byte userCount, byte userId)
    {
         _battle = new Battle(Main.Instance.tick, false, false, userId, _socket, userCount);
    }

    void Update(){
        _battle?.Update();
        _socket.Update();
    }
}