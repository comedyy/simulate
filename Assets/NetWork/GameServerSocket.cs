using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;

public class GameServerSocket : IServerGameSocket, INetEventListener, INetLogger
{
    private NetManager _netServer;
    private List<NetPeer> _ourPeers = new List<NetPeer>();
    private NetDataWriter _dataWriter;
    private int countUser;

    public GameServerSocket(int countUser)
    {
        this.countUser = countUser;
    }

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

        // Debug.LogError(bytes.Length);
        foreach (var peer in _ourPeers)
        {
            peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
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
            if(_ourPeers.Count >= countUser)
            {
                return;
            }

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
        var msgType = (MsgType)reader.PeekInt();
        var msg = reader.GetRemainingBytes();

        OnReceiveMsg(msg);
    }
#endregion

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }

    public void BroadCastBattleStart()
    {
        for(int i = 0; i < _ourPeers.Count; i++)
        {
            var peer = _ourPeers[i];
            
            BattleStartMessage start = new BattleStartMessage(){
                total = _ourPeers.Count,
                userId = i+1
            };
            byte[] bytes = start.ToBytes();
            _dataWriter.Reset();
            _dataWriter.Put(bytes);

            peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
    }

    public int Count => _ourPeers.Count;
}
