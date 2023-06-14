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
        
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SpawnEvent()
        {
            position = new float3(3, 0, 3),
            dir = 0,
            isUser = true,
            isController = true
        });

        entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SpawnEvent()
        {
            position = new float3(3, 0, 1),
            dir = 0,
            isUser = true,
            isController = false
        });

        entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SpawnEvent()
        {
            position = new float3(3, 0, 6),
            dir = 0,
            isUser = true,
            isController = false
        });

        // singetons
        entity = EntityManager.CreateEntity();
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
    }

    public override void Update()
    {
        
    }
}