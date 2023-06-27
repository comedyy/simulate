
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using UnityEngine;

[Serializable]
struct PlaybackInfo
{
    public PlayBackInfoItem[] infos;
}

[Serializable]
struct PlayBackInfoItem
{
    public int frame;
    public List<MessageItem> item;
}

public class LocalFrame
{
    public int frame;
    public float totalSeconds;
    public float preFrameSeconds;
    float _tick;
    public int ReceivedServerFrame;

    List<MessageItem> messageItemList = new List<MessageItem>();
    MessageItem? _messageItem;
    IGameSocket _socket;
    CheckSumMgr _checkSumMgr;
    int checkSumSendCount = 0;
    int _controllerId = 0;

    public void Init(float tick, IGameSocket socket, CheckSumMgr checkSumMgr, int id)
    {
        frame = 5;
        totalSeconds = 0;
        preFrameSeconds = 0;
        _tick = tick;
        _socket = socket;
        _socket.OnReceiveMsg = OnReceive;
        _checkSumMgr = checkSumMgr;
        _controllerId = id;
    }

    FrameHashItem GetHashItem(CheckSum checkSum, int checkSumSendCount)
    {
        #if !HASH_DETAIL
        return new FrameHashItem(){
            hash = checkSum.GetHistory()[checkSumSendCount], 
            listValue = checkSum.GetHistoryDetail()[checkSumSendCount], 
            listEntity = checkSum.GetHistoryDetailOrder()[checkSumSendCount], 
        };
        #else
        return new FrameHashItem(){
            hash = checkSum.GetHistory()[checkSumSendCount], 
            listValue = new List<int>(),
            listEntity = new List<Entity>(), 
        };
        #endif
    }

    public void Update()
    {
        if(_socket == null) return;

        _socket.Update();

        while(checkSumSendCount < _checkSumMgr.Count)
        {
            var positionFrame = GetHashItem(_checkSumMgr.positionChecksum, checkSumSendCount);
            var hpFrame = GetHashItem(_checkSumMgr.hpCheckSum, checkSumSendCount);
            var targetFindFrame = GetHashItem(_checkSumMgr.targetFindCheckSum, checkSumSendCount);
            var preRvo = GetHashItem(_checkSumMgr.preRVO, checkSumSendCount);

            SendHash(checkSumSendCount, 
                positionFrame, 
                hpFrame, 
                targetFindFrame,
                preRvo);
            checkSumSendCount++;
        }

        totalSeconds += Time.deltaTime;
        if(preFrameSeconds + _tick > totalSeconds)
        {
            return;
        }

        preFrameSeconds += _tick;
        frame++;

        if(_messageItem != null)
        {
            _socket.SendMessage(new PackageItem(){
                frame = frame,
                messageItem = _messageItem.Value
            }.ToBytes());
        }
    
        // Debug.LogWarning($"Client:send package  {Time.time}" );
        _messageItem = null;
    }

    public void SetData(MessageItem messageItem)
    {
        _messageItem = messageItem;
    }

    public void SendHash(int frame, FrameHashItem hashPos, FrameHashItem hashHp, FrameHashItem hashFindtarget, FrameHashItem preRvo)
    {
        _socket.SendMessage(new FrameHash(){
            frame = frame, hashPos = hashPos, hashFindtarget = hashFindtarget, hashHp = hashHp, preRvo = preRvo, id = _controllerId
        }.ToBytes());
    }

    Dictionary<int , List<MessageItem>> _allMessage = new Dictionary<int, List<MessageItem>>();

    internal void OnReceive(byte[] bytes)
    {
        ServerPackageItem item = new ServerPackageItem();
        item.From(bytes);

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

        if(list != null && list.Count > 0)
        {
            _allMessage.Add(frame, list);
        }

        ReceivedServerFrame = frame;
        // Debug.LogError($"Client:receive package {frame} {Time.time} count:{_allMessage.Count}" );
    }

    public List<MessageItem> GetFrameInput(int frame)
    {
        if(_allMessage.TryGetValue(frame, out var list))
        {
            // if(list != null && list.Count > 0)
            // {
            //     UnityEngine.Debug.LogError($"{frame} {list[0].pos} {Time.time}");
            // }

            return list;
        }

        return null;
    }

    string path = Application.dataPath + "/../playback.json";
    public void SavePlayback()
    {
        var infos = new PlayBackInfoItem[_allMessage.Count];
        PlaybackInfo item = new PlaybackInfo(){
            infos = infos
        };

        int count = 0;
        foreach(var x in _allMessage)
        {
            infos[count++] = new PlayBackInfoItem(){
                frame = x.Key, item = x.Value
            };
        }
        var json = JsonUtility.ToJson(item, true);

        File.WriteAllText(path, json);
    }

    internal void LoadPlayBackInfo(bool load)
    {
        ReceivedServerFrame = int.MaxValue;
        if(!load) return;
        
        if(File.Exists(path))
        {
            var context = File.ReadAllText(path);
            var pkgInfo = JsonUtility.FromJson<PlaybackInfo>(context);

            _allMessage.Clear();
            foreach (var item in pkgInfo.infos)
            {
                _allMessage.Add(item.frame, item.item);
            }

            Debug.Log("load playback info");
        }
        else
        {
            Debug.Log("not found playback info");
        }
    }
}