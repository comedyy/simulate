
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ConnectResult
{
    Connecting,
    Refuse,
    Connnected,
}

public interface IServerGameSocket : IGameSocket
{
    void BroadCastBattleStart();
    int Count{get;}
}

public interface IClientGameSocket : IGameSocket
{
    Action<byte, byte> OnStartBattle{get;set;}
    ConnectResult connectResult{get;}
}


public interface IGameSocket : IMessageSendReceive, ILifeCircle
{
    
}

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


struct SendItem
{
    public float addTime;
    public byte[] bytes;
}

struct ReceiveItem
{
    public float addTime;
    public byte[] bytes;
}

interface IConnectionCount
{
    int Count{get;}
}

public class DumpGameServerSocket : IServerGameSocket, IConnectionCount
{
    public float HALF_PING => pingSec / 2f;

    public Action<byte[]> OnReceiveMsg{get;set;}

    public int Count => BroadCastEvent.Count;

    List<Action<byte[]>> BroadCastEvent = new List<Action<byte[]>>();
    public bool AddBroadCastEvent(Action<byte[]> action)
    {
        if(BroadCastEvent.Count >= totalConnection) return false;

        BroadCastEvent.Add(action);
        return true;
    }

    Queue<SendItem> _sendList = new Queue<SendItem>();
    Queue<ReceiveItem> _receiveList = new Queue<ReceiveItem>();

    private float pingSec;
    private int totalConnection;

    public static DumpGameServerSocket Instance{get; private set;}

    public DumpGameServerSocket(float pingSec, int totalConnection)
    {
        this.pingSec = pingSec;
        this.totalConnection = totalConnection;
        Instance = this;
    }

    // public void Init(Action<PackageItem> SendMsg, Action<ServerPackageItem> FrameCallback)
    // {
    //     this.SendMsg = SendMsg;
    //     this.FrameCallback = FrameCallback;
    // }

    public void Update()
    {
        while (_receiveList.Count > 0)
        {
            if(_receiveList.Peek().addTime < Time.time)
            {
                var item = _receiveList.Dequeue().bytes;
                OnReceiveMsg?.Invoke(item);
            }
            else
            {
                break;
            }
        }

        while (_sendList.Count > 0)
        {
            if(_sendList.Peek().addTime < Time.time)
            {
                var item = _sendList.Dequeue().bytes;
                foreach(var x in BroadCastEvent)
                {
                    x.Invoke(item);
                }
            }
            else
            {
                break;
            }
        }
    }

    public void SendMessage(byte[] bytes)
    {
        _sendList.Enqueue(new SendItem(){
            addTime = Time.time + HALF_PING, bytes = bytes
        });
    }

    public void ReceiveMessageFromClient(byte[] bytes)
    {
        _receiveList.Enqueue(new ReceiveItem(){
            addTime = Time.time + HALF_PING, bytes = bytes
        });
    }

    public void Start()
    {
    }


    public void OnDestroy()
    {
    }

    public void BroadCastBattleStart()
    {
        for(int i = 0; i < BroadCastEvent.Count; i++)
        {
            BattleStartMessage start = new BattleStartMessage(){
                total = BroadCastEvent.Count,
                userId = i+1
            };
            BroadCastEvent[i](start.ToBytes());
        }
    }
}