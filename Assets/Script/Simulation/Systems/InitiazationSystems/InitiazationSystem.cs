using Unity.Entities;
using Unity.Mathematics;
using System;

public class InitiazationSystem : ComponentSystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SpawnEvent()
        {
            position = new float3(3, 0, 3),
            dir = quaternion.identity,
            isUser = true,
            isController = true
        });

        entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SpawnEvent()
        {
            position = new float3(3, 0, 1),
            dir = quaternion.identity,
            isUser = true,
            isController = false
        });

        // create random
        EntityManager.AddComponentData(EntityManager.CreateEntity(), new RandomComponent(){
            random = new System.Random(1)
        });
    }

    public override void Update()
    {
        
    }
}