
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
    Action<PackageItem> SendMsg;
    public int ReceivedServerFrame;

    List<MessageItem> messageItemList = new List<MessageItem>();

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
        if(SendMsg == null) return;

        totalSeconds += Time.deltaTime;
        if(preFrameSeconds + _tick > totalSeconds)
        {
            return;
        }

        preFrameSeconds += _tick;
        frame++;

        if(messageItemList.Count > 0)
        {
            foreach(var x in messageItemList)
            {
                SendMsg(new PackageItem(){
                    frame = frame,
                    messageItem = x
                });
            }
            
            // Debug.LogWarning($"Client:send package  {Time.time}" );
        }

        messageItemList.Clear();
    }

    public void SetData(MessageItem messageItem)
    {
        messageItemList.Add(messageItem);
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

    internal void LoadPlayBackInfo()
    {
        if(File.Exists(path))
        {
            var context = File.ReadAllText(path);
            var pkgInfo = JsonUtility.FromJson<PlaybackInfo>(context);

            _allMessage.Clear();
            foreach (var item in pkgInfo.infos)
            {
                _allMessage.Add(item.frame, item.item);
            }
            ReceivedServerFrame = int.MaxValue;

            Debug.Log("load playback info");
        }
        else
        {
            Debug.Log("not found playback info");
        }
    }
}