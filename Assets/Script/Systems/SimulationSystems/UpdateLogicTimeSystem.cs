using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class UpdateLogicTimeSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        EntityManager.AddComponentData(EntityManager.CreateEntity(), new LogicTime(){
            deltaTime = 0.1f
        });
    }

    protected override void OnUpdate()
    {
        var logicTime = GetSingleton<LogicTime>();
        logicTime.escaped += logicTime.deltaTime;
        logicTime.frameCount ++;
        SetSingleton(logicTime);
    }
}