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
            position = float3.zero,
            dir = quaternion.identity,
            isUser = true
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