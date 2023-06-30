using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    public Button _hostButton;
    public Button _startButton;
    public Button _joinButton;
    public Text _info;

    Room _room;

//    public static string ip = "192.168.2.14";
    public static string ip = "127.0.0.1";
    public bool userAutoMove = false;

    void OnGUI()
    {
        ip = GUI.TextField(new Rect(100, 100, 100, 100), ip);

        GUI.color = Color.black;
        userAutoMove = GUI.Toggle(new Rect(100, 0, 100, 100), userAutoMove, "ai控制操作");
    }

    // Start is called before the first frame update
    void Start()
    {
        _hostButton.onClick.AddListener(OnClickHost);
        _joinButton.onClick.AddListener(OnClickJoin);
        _startButton.onClick.AddListener(OnClickStart);

        _startButton.gameObject.SetActive(false);
    }

    private void OnClickStart()
    {
        _room.StartBattle();
    }

    private void OnClickJoin()
    {
        // 创建一个单位，并加入到游戏中。
        GameObject o = new GameObject("newPlayer");
        var user = o.AddComponent<GameUser>();
        var isLocalClient = _room != null;
        user.Init(isLocalClient, userAutoMove);

        _hostButton.gameObject.SetActive(false);
    }

    private void OnClickHost()
    {
        IServerGameSocket socket;
        if(Main.Instance.useRealNetwork)
        {
            socket = new GameServerSocket(Main.Instance.countUser);
        }
        else
        {
            socket = new DumpGameServerSocket(0, Main.Instance.countUser);
        }

        _room = new GameObject("server").AddComponent<Room>();
        _room.Init(socket);


        OnClickJoin();

        _hostButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if(_room != null)
        {
            _info.text = $"{_room.UserCount}/{Main.Instance.countUser}";
        }
        else
        {
            _info.text = $"本地玩家数量{UnityEngine.Object.FindObjectsOfType<GameUser>().Length}";
        }

        _startButton.gameObject.SetActive(_room != null);
    }
}
