
using System;
using System.Collections.Generic;

public class RoomMember
{
    public string name;
    public int id;
}

public class Room
{
    // List<RoomMember> _allMembers = new List<RoomMember>();
    Server _server;

    public Room()
    {
        var tick = 1f / Main.Instance.LogicFrameCount;
        DumpGameServerSocket socket = new DumpGameServerSocket(0, Main.Instance.countUser);
        _server = new Server(tick, socket);
    }

    public int UserCount => _server.ServerDumpSocket.Count;

    internal void StartBattle()
    {
        _server.StartBattle();
        
        Main.Instance.StartBattle(_server);
    }
}