﻿using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

public class GameClientSocket : IClientGameSocket, INetEventListener
{
    private NetManager _netClient;
    private NetDataWriter _dataWriter;
    public Action<byte, byte> OnStartBattle{get;set;}

    bool _localSocket = false;
    public GameClientSocket(bool localSocket)
    {
        _localSocket = localSocket;
    }

#region ILifeCircle
    public void Start()
    {
        _netClient = new NetManager(this);
        _dataWriter = new NetDataWriter();
        if(!_localSocket)
        {
            _netClient.UnconnectedMessagesEnabled = true;
        }
        _netClient.UpdateTime = 15;
        _netClient.Start();

        if(_localSocket)
        {
            _netClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000), "sample_app");
        }
    }

    public void Update()
    {
        _netClient.PollEvents();

        if(!_localSocket)
        {
            var peer = _netClient.FirstPeer;
            var isPeerValid = peer != null && peer.ConnectionState == ConnectionState.Connected;
            if(!isPeerValid)
            {
                _netClient.SendBroadcast(new byte[] {1}, 5000);
            }
        }
    }

    public void OnDestroy()
    {
        if (_netClient != null)
            _netClient.Stop();
    }
#endregion

#region IMessageSendReceive
    public Action<byte[]> OnReceiveMsg{get;set;}

    public ConnectResult connectResult{get; private set;}

    public void SendMessage(byte[] bytes)
    {
        var peer = _netClient.FirstPeer;
        if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        {
            _dataWriter.Reset();
            _dataWriter.Put(bytes);
            peer.Send(_dataWriter, DeliveryMethod.Sequenced);
        }
    }
#endregion

#region INetEventListener
    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
        connectResult = ConnectResult.Connnected;
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[CLIENT] We received error " + socketErrorCode);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var msgType = (MsgType)reader.PeekByte();
        var msg = reader.GetRemainingBytes();
        if(msgType == MsgType.ServerFrameMsg)
        {
            OnReceiveMsg(msg);
        }
        else if(msgType == (MsgType.StartBattle))
        {
            var startBattle = new BattleStartMessage();
            startBattle.From(msg);
            OnStartBattle((byte)startBattle.total, (byte)startBattle.userId);
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0)
        {
            if(reader.GetInt() == 1)
            {
                Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
                _netClient.Connect(remoteEndPoint, "sample_app");
            }
            else if (reader.GetInt() == 2)
            {
                connectResult = ConnectResult.Refuse;
            }
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
    }
    #endregion
}
