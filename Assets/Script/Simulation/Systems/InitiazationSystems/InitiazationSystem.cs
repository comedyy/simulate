using Unity.Entities;
using Unity.Mathematics;
using System;

public class InitiazationSystem : ComponentSystemBase
{
    public static float logicFrameInterval;

    public void Init(float v)
    {
  
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        

        for(int i = 0; i < 1; i++)
        {
            EntityManager.AddComponentData(EntityManager.CreateEntity(), new SpawnEvent()
            {
                position = new float3(3, 0, 3),
                dir = 0,
                id = i + 1,
                isUser = true,
            });
        }
        

        // singetons
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new LogicTime(){
            deltaTime = logicFrameInterval
        });

        // create random
        EntityManager.AddComponentData(entity, new RandomComponent(){
            random = new System.Random(1)
        });

        // create RVO
        var rvoSimulator = new RVO.Simulator();
        rvoSimulator.setTimeStep(logicFrameInterval);
        EntityManager.AddComponentObject(entity, new RvoSimulatorComponet(){
            rvoSimulator = rvoSimulator
        });

        EntityManager.AddComponent<ControllerHolder>(EntityManager.CreateEntity());
    }

    public override void Update()
    {
        
    }
}