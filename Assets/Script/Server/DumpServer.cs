using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DumpServer
{
    public int frame;
    public float totalSeconds;
    public float preFrameSeconds;
    float _tick;
    Action<ServerPackageItem> FrameCallback;

    Dictionary<int , List<MessageItem>> _allMessage = new Dictionary<int, List<MessageItem>>();

    public void Init(float tick, Action<ServerPackageItem> frameCallback)
    {
        frame = -5;
        totalSeconds = 0;
        preFrameSeconds = 0;
        _tick = tick;
        _allMessage.Clear();
        FrameCallback = frameCallback;
    }

    public void Update()
    {
        totalSeconds += Time.deltaTime;
        if(preFrameSeconds + _tick > totalSeconds)
        {
            return;
        }

        preFrameSeconds += _tick;

        if(_allMessage.TryGetValue(frame, out var list))
        {
            _allMessage.Remove(frame);
        }

        BroadCastMsg(list);

        frame++;
    }

    public void AddMessage(PackageItem packageItem)
    {
        var currentFrame = packageItem.frame;
        if(currentFrame < frame)
        {
            currentFrame = frame;
        }

        if(currentFrame > frame + 10)
        {
            Debug.LogError("frame > 10");
            return;
        }

        if(!_allMessage.TryGetValue(currentFrame, out var list))
        {
            list = new List<MessageItem>();
            _allMessage.Add(currentFrame, list);
        }

        list.Add(packageItem.messageItem);
    }

    private void BroadCastMsg(List<MessageItem> list)
    {
        FrameCallback(new ServerPackageItem(){
            frame = frame, list = list
        });
    }
}