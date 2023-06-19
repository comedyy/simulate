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
        gameObject.SetActive(false);
    }

    private void OnClickJoin()
    {
        // 创建一个单位，并加入到游戏中。
        GameObject o = new GameObject("newPlayer");
        o.AddComponent<GameUser>();
    }

    private void OnClickHost()
    {
        _room = new Room();

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
