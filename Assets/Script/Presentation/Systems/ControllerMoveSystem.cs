using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ControllerMoveSystem : ComponentSystem
{
    internal LocalFrame localServer;

    protected override void OnUpdate()
    {   
        var userEntity = GetSingleton<ControllerHolder>().controller;
        if(!EntityManager.Exists(userEntity)) return;

        var controllerHolder = GetSingleton<ControllerHolder>();
        var controllerEntity = controllerHolder.controller;

        if(!EntityManager.Exists(controllerEntity)) return;

        var binding = EntityManager.GetComponentData<GameObjectBindingComponent>(controllerEntity);
        if(binding.obj == null) return;
        int controllerId = EntityManager.GetComponentData<UserComponnet>(controllerEntity).id;

        var tranCom = binding.obj.transform;
        if(controllerId == 1)   // 用户
        {
             var moveSpeedComponent = EntityManager.GetComponentData<MoveSpeedComponent>(controllerEntity);

            var angle = GetAngle();

            if(angle < 0) return;

            tranCom.rotation = math.nlerp(tranCom.rotation, quaternion.RotateY(angle), 0.05f);
            var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
            tranCom.position += (Vector3)(Time.DeltaTime * dir * moveSpeedComponent.speed);

            localServer.SetData(new MessageItem(){
                pos = tranCom.position, id = controllerId
            });
        }
        else    // ai
        {
            UpdateOtherUser(controllerId);
        }
    }

    private void UpdateOtherUser(int controllerId)
    {
        float3 firstPos = default;
        Entity firstEntity = default;

        // simulate other role behavior
        // 保持跟玩家的相对位置。
        var list = GetSingleton<UserListComponent>().allUser;
        for(int i = 0; i < list.length; i++)
        {
            if(EntityManager.GetComponentData<UserComponnet>(list[i]).id == 1)
            {
                firstEntity = list[i];
                firstPos = EntityManager.GetComponentData<LTransformComponet>(firstEntity).position;
                continue;
            }
        }

        for(int i = 0; i < list.length; i++)
        {
            var id = EntityManager.GetComponentData<UserComponnet>(list[i]).id;
            if(id != controllerId)
            {
                continue;
            }

            if(list[i] == firstEntity)
            {
                continue;
            }

            var entity = list[i];
            var shouldBePos = firstPos + EntityManager.GetComponentData<UserAiComponent>(entity).offsetToController;
            var targetPos = shouldBePos;

            int userId = EntityManager.GetComponentData<UserComponnet>(entity).id;
            localServer.SetData(new MessageItem(){
                pos = targetPos, id = userId
            });
        }
    }

    static float GetAngle()
    {
        float angle = -1;
        if(Input.GetKey(KeyCode.W)){
            angle = 0;            
        }
        else if(Input.GetKey(KeyCode.A))
        {
            angle = math.radians(270); 
        }
        else if(Input.GetKey(KeyCode.D))
        {
            angle = math.radians(90);
        }
        else if(Input.GetKey(KeyCode.S))
        {
            angle = math.radians(180);
        }

        return angle;
    }
}