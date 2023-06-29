using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class VCameraFollowSystem : SystemBase
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
        if(!EntityManager.Exists(userEntity)) return;
        
        if(Application.isEditor && EntityManager.GetComponentData<UserComponnet>(userEntity).id != 1)
        {
            return;
        }
        
        var obj = EntityManager.GetComponentObject<GameObjectBindingComponent>(userEntity).obj;
        if(obj == null) return;
        var pos = obj.transform.position;
        _camera.transform.position = new float3(pos) + new float3(0, 10, -5);
    }
}