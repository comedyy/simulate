using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    public Button _hostButton;
    public Button _joinButton;
    public InputField _inputField;

    Room _room;

    // Start is called before the first frame update
    void Start()
    {
        _hostButton.onClick.AddListener(OnClickHost);
        _hostButton.onClick.AddListener(OnClickJoin);
    }

    private void OnClickJoin()
    {
        string ip = _inputField.text;
    }

    private void OnClickHost()
    {
        string ip = _inputField.text;
    }
}
