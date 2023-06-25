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
            var pos = new fp3(){
                x = fp.Create(message.pos.x / 100, (message.pos.x % 100 ) * 1000),
                y = fp.Create(message.pos.y / 100, (message.pos.y % 100 ) * 1000),
                z = fp.Create(message.pos.z / 100, (message.pos.z % 100 ) * 1000),
            };
            var diff = pos - preTransform.position;
            var dir = fpMath.normalize(diff);
            EntityManager.SetComponentData(entity, new LTransformComponet(){
                rotation = dir,
                position = pos
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