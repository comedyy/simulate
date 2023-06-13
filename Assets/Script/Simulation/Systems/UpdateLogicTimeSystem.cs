using Unity.Entities;
using Unity.Mathematics;


public class UpdateLogicTimeSystem : ComponentSystem
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