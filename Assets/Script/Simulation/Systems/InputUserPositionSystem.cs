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
            EntityManager.SetComponentData(message.entity, new LMoveByPosComponent(){
                pos = message.pos
            });
        }
    }
}