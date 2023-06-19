
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomMember
{
    public string name;
    public int id;
}

public class Room : MonoBehaviour
{
    // List<RoomMember> _allMembers = new List<RoomMember>();
    Server _server;

    public void Init(IServerGameSocket socket)
    {
        _server = new Server(Main.Instance.tick, socket);
    }

    void Update()
    {
        _server.Update();
    }

    public int UserCount => _server.PeerCount;

    internal void StartBattle()
    {
        _server.StartBattle();
    }

    void OnDestroy()
    {
        _server.Destroy();
    }
}