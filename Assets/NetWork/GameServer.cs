using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;

public interface IMessageSendReceive
{
    void SendMessage(byte[] bytes);
    Action<byte[]> OnReceiveMsg{get;set;}
}

public interface ILifeCircle
{
    void Start();
    void Update();
    void OnDestroy();
}

public class GameServer : IMessageSendReceive, ILifeCircle, INetEventListener, INetLogger
{
    private NetManager _netServer;
    private List<NetPeer> _ourPeers = new List<NetPeer>();
    private NetDataWriter _dataWriter;

#region ILifeCircle
    public void Start()
    {
        NetDebug.Logger = this;
        _dataWriter = new NetDataWriter();
        _netServer = new NetManager(this);
        _netServer.Start(5000);
        _netServer.BroadcastReceiveEnabled = true;
        _netServer.UpdateTime = 15;
    }

    public void Update()
    {
        _netServer.PollEvents();
    }

    public void OnDestroy()
    {
        NetDebug.Logger = null;
        if (_netServer != null)
            _netServer.Stop();
    }
#endregion
    
#region IMessageSendReceive
    public Action<byte[]> OnReceiveMsg{get;set;}
    public void SendMessage(byte[] bytes)
    {
        if(_ourPeers.Count == 0) return;

        _dataWriter.Reset();
        _dataWriter.Put(bytes);

        foreach (var peer in _ourPeers)
        {
            peer.Send(_dataWriter, DeliveryMethod.Sequenced);
        }
    }
#endregion

#region INetEventListener
    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[SERVER] We have new peer " + peer.EndPoint);
        _ourPeers.Add(peer);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[SERVER] error " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            Debug.Log("[SERVER] Received discovery request. Send discovery response");
            NetDataWriter resp = new NetDataWriter();
            resp.Put(1);
            _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey("sample_app");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        _ourPeers.Remove(peer);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var msg = reader.GetRemainingBytes();
        OnReceiveMsg(msg);
    }
#endregion

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }
}
