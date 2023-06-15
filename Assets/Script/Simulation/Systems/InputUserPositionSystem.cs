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

        foreach (var message in list)
        {
            var preTransform = EntityManager.GetComponentData<LTransformComponet>(message.entity);
            var diff = message.pos - preTransform.position;
            var dir = math.normalize(diff);
            EntityManager.SetComponentData(message.entity, new LTransformComponet(){
                rotation = quaternion.LookRotation(dir, new float3(0, 1, 0)),
                position = message.pos
            });

            var com = EntityManager.GetComponentData<VLerpTransformCopmnet>(message.entity);
            com.lerpTime = 0;
            EntityManager.SetComponentData(message.entity, com);
        }
    }
}