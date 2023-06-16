using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;


public class InputUserPositionSystem : ComponentSystem
{
    public Func<int, List<MessageItem>> GetAllMessage;
    
    protected override void OnUpdate()
    {
        var list = GetAllMessage.Invoke(GetSingleton<LogicTime>().frameCount);
        if(list == null) return;
        var listUser = GetSingleton<UserListComponent>().allUser;

        foreach (var message in list)
        {
            var entity = GetEntityById(message.id);
            if(!EntityManager.Exists(entity))
            {
                continue;
            }

            var preTransform = EntityManager.GetComponentData<LTransformComponet>(entity);
            var diff = message.pos - preTransform.position;
            var dir = math.normalize(diff);
            EntityManager.SetComponentData(entity, new LTransformComponet(){
                rotation = quaternion.LookRotation(dir, new float3(0, 1, 0)),
                position = message.pos
            });

            var com = EntityManager.GetComponentData<VLerpTransformCopmnet>(entity);
            com.lerpTime = 0;
            EntityManager.SetComponentData(entity, com);
        }
    }

    private Entity GetEntityById(int id)
    {
        var listUser = GetSingleton<UserListComponent>().allUser;
        for(int i = 0; i < listUser.length; i++)
        {
            if(EntityManager.GetComponentData<UserComponnet>(listUser[i]).id == id)
            {
                return listUser[i];
            }
        }

        return default;
    }
}