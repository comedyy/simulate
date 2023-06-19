
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IServerGameSocket : IGameSocket
{
    void BroadCastBattleStart();
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

public struct PackageItem
{
    public int frame;
    public MessageItem messageItem;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write(frame);
        writer.Write(messageItem.id);
        writer.Write((int)(messageItem.pos.x * 100));
        writer.Write((int)(messageItem.pos.y * 100));
        writer.Write((int)(messageItem.pos.z * 100));

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        frame = reader.ReadInt32();
        messageItem = new MessageItem(){
            id = reader.ReadInt32(),
            pos = new Unity.Mathematics.float3()
            {
                x = reader.ReadInt32() / 100f,
                y = reader.ReadInt32() / 100f,
                z = reader.ReadInt32() / 100f,
            }
        };
    }
}

public struct ServerPackageItem
{
    public int frame;
    public List<MessageItem> list;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write(frame);
        if(list != null) 
        {
            writer.Write(list.Count);
            for(int i = 0; i < list.Count; i++)
            {
                var messageItem = list[i];
                writer.Write(messageItem.id);
                writer.Write((int)(messageItem.pos.x * 100));
                writer.Write((int)(messageItem.pos.y * 100));
                writer.Write((int)(messageItem.pos.z * 100));
            }
        }
        else
        {
            writer.Write(0);
        }

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        frame = reader.ReadInt32();
        var count = reader.ReadInt32();
        if(count > 0)
        {
            list = new List<MessageItem>();
            for(int i = 0; i < count; i++)
            {
                var messageItem = new MessageItem(){
                    id = reader.ReadInt32(),
                    pos = new Unity.Mathematics.float3()
                    {
                        x = reader.ReadInt32() / 100f,
                        y = reader.ReadInt32() / 100f,
                        z = reader.ReadInt32() / 100f,
                    }
                };
                list.Add(messageItem);
            }
        }
    }
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
            BroadCastEvent[i](new byte[]{(byte)BroadCastEvent.Count, (byte)(i+1)});
        }
    }
}