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

        // var dir = 90;
        // var q = quaternion.RotateY(math.radians(dir));
        // var x = math.rotate(q, new float3(0, 0, 1));
        // Debug.LogError(q + " " + dir + " " + x);
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
