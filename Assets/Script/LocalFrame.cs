
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class LocalFrame
{
    public int frame;
    public float totalSeconds;
    public float preFrameSeconds;
    float _tick;
    Action<PackageItem> SendMsg;
    public int ReceivedServerFrame;

    MessageItem messageItem;

    public void Init(float tick, Action<PackageItem> SendMsg)
    {
        frame = 5;
        totalSeconds = 0;
        preFrameSeconds = 0;
        _tick = tick;
        this.SendMsg = SendMsg;
    }

    public void Update()
    {
        totalSeconds += Time.deltaTime;
        if(preFrameSeconds + _tick > totalSeconds)
        {
            return;
        }

        preFrameSeconds += _tick;
        frame++;

        if(messageItem.entity != Entity.Null)
        {
            SendMsg(new PackageItem(){
                frame = frame,
                messageItem = messageItem
            });
            messageItem = default;
        }
    }

    public void SetData(MessageItem messageItem)
    {
        this.messageItem = messageItem;
    }

    Dictionary<int , List<MessageItem>> _allMessage = new Dictionary<int, List<MessageItem>>();

    internal void OnReceive(ServerPackageItem item)
    {
        int frame = item.frame;
        if(frame <= 0) return;

        var list = item.list;
        if(frame <= ReceivedServerFrame)
        {
            Debug.LogError($"frame <= frameServer {frame} {ReceivedServerFrame}");
            return;
        }

        if(_allMessage.ContainsKey(frame))
        {
            Debug.LogError("_allMessage.ContainsKey(frame)");
            return;
        }

        if(list != null)
        {
            _allMessage.Add(frame, list);
        }

        ReceivedServerFrame = frame;
    }

    public List<MessageItem> GetFrameInput(int frame)
    {
        if(_allMessage.TryGetValue(frame, out var list))
        {
            return list;
        }

        return null;
    }
}