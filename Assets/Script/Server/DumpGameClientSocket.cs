
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DumpGameClientSocket : IGameSocket
{
    public Action<byte[]> OnReceiveMsg{get;set;}

    Queue<SendItem> _sendList = new Queue<SendItem>();
    Queue<ReceiveItem> _receiveList = new Queue<ReceiveItem>();
    DumpGameServerSocket _serverSocket;
    public Action<byte, byte> OnStartBattle{get;set;}

    public bool Connect()
    {
        this._serverSocket = DumpGameServerSocket.Instance;
        return this._serverSocket.AddBroadCastEvent(OnReceiveMessageFromServer);
    }

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
            addTime = Time.time, bytes = bytes
        });
    }

    void OnReceiveMessageFromServer(byte[] bytes)
    {
        if(bytes.Length == 2) // startBattle
        {
            OnStartBattle( bytes[0], bytes[1] );
            return;
        }

        _receiveList.Enqueue(new ReceiveItem(){
            addTime = Time.time, bytes = bytes
        });
    }

    public void Start()
    {
    }


    public void OnDestroy()
    {
    }
}