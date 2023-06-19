using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Server
{
    public int frame;
    public float totalSeconds;
    public float preFrameSeconds;
    float _tick;
    // event Action<byte[]> FrameCallback;
    IGameSocket _socket;

    Dictionary<int , List<MessageItem>> _allMessage = new Dictionary<int, List<MessageItem>>();
    List<MessageItem> _allMessage1 = new List<MessageItem>();

    public DumpGameServerSocket ServerDumpSocket => _socket as DumpGameServerSocket;

    public Server(float tick, IGameSocket socket)
    {
        frame = 1;
        totalSeconds = 0;
        preFrameSeconds = 0;
        _tick = tick;
        _allMessage.Clear();
        _socket = socket;
        _socket.OnReceiveMsg = AddMessage;
    }

    // public void AddCallback(Action<byte[]> frameCallback)
    // {
    //     FrameCallback += frameCallback;
    // }

    public void Update()
    {
        _socket.Update();
        
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

    public void AddMessage(byte[] bytes)
    {
        PackageItem packageItem = new PackageItem();
        packageItem.From(bytes);

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
        // Debug.LogWarning($"Server:Recive package  {Time.time}" );
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
        _socket.SendMessage(new ServerPackageItem(){
            frame = frame, list = list
        }.ToBytes());
    }
}