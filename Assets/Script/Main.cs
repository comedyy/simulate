using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Main : MonoBehaviour
{
    BattleWorld _world;
    // Start is called before the first frame update
    void Start()
    {
        _world = new BattleWorld("new");
    }

    // Update is called once per frame
    void Update()
    {
        _world.Update();
    }

    void OnGUI()
    {

    }
}
