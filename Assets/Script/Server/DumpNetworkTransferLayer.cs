
using System;
using System.Collections.Generic;
using UnityEngine;

public struct PackageItem
{
    public int frame;
    public MessageItem messageItem;
}

public struct ServerPackageItem
{
    public int frame;
    public List<MessageItem> list;
}

struct SendItem
{
    public float addTime;
    public PackageItem item;
}

struct ReceiveItem
{
    public float addTime;
    public ServerPackageItem items;
}

public class DumpNetworkTransferLayer
{
    public float HALF_PING => pingSec / 2f;

    Queue<SendItem> _sendList = new Queue<SendItem>();
    Queue<ReceiveItem> _receiveList = new Queue<ReceiveItem>();

    Action<PackageItem> SendMsg;
    Action<ServerPackageItem> FrameCallback;
    private float pingSec;

    public DumpNetworkTransferLayer(float pingSec)
    {
        this.pingSec = pingSec;
    }

    public void Init(Action<PackageItem> SendMsg, Action<ServerPackageItem> FrameCallback)
    {
        this.SendMsg = SendMsg;
        this.FrameCallback = FrameCallback;
    }

    public void Update(float pingSec)
    {
        this.pingSec = pingSec;

        while (_sendList.Count > 0)
        {
            if(_sendList.Peek().addTime < Time.time)
            {
                SendMsg(_sendList.Dequeue().item);
            }
            else
            {
                break;
            }
        }


        while (_receiveList.Count > 0)
        {
            if(_receiveList.Peek().addTime < Time.time)
            {
                FrameCallback(_receiveList.Dequeue().items);
            }
            else
            {
                break;
            }
        }
    }

    public void Send(PackageItem item)
    {
        _sendList.Enqueue(new SendItem(){
            addTime = Time.time + HALF_PING, item = item
        });
    }

    public void Receive(ServerPackageItem list)
    {
        _receiveList.Enqueue(new ReceiveItem(){
            addTime = Time.time + HALF_PING, items = list
        });
    }
}