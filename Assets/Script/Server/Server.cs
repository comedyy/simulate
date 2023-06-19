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
    IServerGameSocket _socket;

    Dictionary<int , List<MessageItem>> _allMessage = new Dictionary<int, List<MessageItem>>();
    List<MessageItem> _allMessage1 = new List<MessageItem>();

    public int PeerCount => _socket.Count;
    HashChecker _hashChecker;

    public Server(float tick, IServerGameSocket socket)
    {
        frame = 1;
        totalSeconds = 0;
        preFrameSeconds = 0;
        _tick = tick;
        _allMessage.Clear();
        _socket = socket;
        _socket.Start();
        _socket.OnReceiveMsg = AddMessage;
    }

    // public void AddCallback(Action<byte[]> frameCallback)
    // {
    //     FrameCallback += frameCallback;
    // }

    public void Update()
    {
        _socket.Update();
        
        if(!start) return;
        
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
        var msgType = bytes[0];
        if(msgType == (byte)MsgType.HashMsg)
        {
            FrameHash hash = new FrameHash();
            hash.From(bytes);
            _hashChecker.AddHash(hash);
            return;
        }

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
        // Debug.LogError("AddMessage" + packageItem.frame);
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
        // Debug.LogError(list == null ? 0 : list.Count);
        // Debug.LogError($"Server:Send package  {frame} {Time.time}" );
        _socket.SendMessage(new ServerPackageItem(){
            frame = frame, list = list
        }.ToBytes());
    }

    bool start = false;
    public void StartBattle()
    {
        _hashChecker = new HashChecker(_socket.Count);
        start = true;
        _socket.BroadCastBattleStart();
    }

    public void Destroy()
    {
        _socket.OnDestroy();
    }
}