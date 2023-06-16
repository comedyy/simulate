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
    event Action<ServerPackageItem> FrameCallback;

    Dictionary<int , List<MessageItem>> _allMessage = new Dictionary<int, List<MessageItem>>();
    List<MessageItem> _allMessage1 = new List<MessageItem>();

    public DumpServer(float tick)
    {
        frame = 1;
        totalSeconds = 0;
        preFrameSeconds = 0;
        _tick = tick;
        _allMessage.Clear();
    }

    public void AddCallback(Action<ServerPackageItem> frameCallback)
    {
        FrameCallback += frameCallback;
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

        BroadCastMsg(_allMessage1);
        _allMessage1 = null;

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

        if(_allMessage1 ==  null) 
        {
            _allMessage1 = new List<MessageItem>();
        }

        _allMessage1.Add(packageItem.messageItem);
        Debug.LogWarning($"Server:Recive package  {Time.time}" );
        // if(!_allMessage.TryGetValue(currentFrame, out var list))
        // {
        //     list = new List<MessageItem>();
        //     _allMessage.Add(currentFrame, list);
        // }

        // list.Add(packageItem.messageItem);
    }

    private void BroadCastMsg(List<MessageItem> list)
    {
        // Debug.LogError($"Server:Send package  {frame} {Time.time}" );
        FrameCallback(new ServerPackageItem(){
            frame = frame, list = list
        });
    }
}