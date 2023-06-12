using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class VCameraFollowSystem : SystemBase
{
    Camera _camera;
    protected override void OnCreate()
    {
        base.OnCreate();
        _camera = Camera.main;
    }

    protected override void OnUpdate()
    {
        var userEntity = GetSingleton<ControllerHolder>().controller;
        var pos = EntityManager.GetComponentObject<GameObjectBindingComponent>(userEntity).obj.transform.position;
        _camera.transform.position = new float3(pos) + new float3(0, 10, 0);
    }
}