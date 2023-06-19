
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DumpGameClientSocket : IGameSocket
{
    public float HALF_PING => pingSec / 2f;

    public Action<byte[]> OnReceiveMsg{get;set;}

    Queue<SendItem> _sendList = new Queue<SendItem>();
    Queue<ReceiveItem> _receiveList = new Queue<ReceiveItem>();

    private float pingSec;
    DumpGameServerSocket _serverSocket;

    public DumpGameClientSocket(float pingSec, DumpGameServerSocket serverSocket)
    {
        this.pingSec = pingSec;
        this._serverSocket = serverSocket;
        this._serverSocket.BroadCastEvent += OnReceiveMessageFromServer;
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
                _serverSocket.ReceiveMessageFromClient(item);
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

    void OnReceiveMessageFromServer(byte[] bytes)
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
}