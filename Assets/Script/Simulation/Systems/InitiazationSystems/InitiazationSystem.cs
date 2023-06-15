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
                dir = new float3(1, 0, 0),
                id = i + 1,
                isUser = true,
                atk = 10,
                hp = 3000
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

        EntityManager.AddComponent<ControllerHolder>(entity);
        EntityManager.AddComponentData<UserListComponent>(entity, new UserListComponent(){
            allUser = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Entity>(8, Unity.Collections.Allocator.Persistent)
        });

        EntityManager.AddBuffer<SpawnEventComponent>(entity);
        EntityManager.AddBuffer<DeSpawnEventComponent>(entity);
        EntityManager.AddBuffer<HurtComponent>(entity);
        EntityManager.AddBuffer<VHurtComponent>(entity);
    }

    public override void Update()
    {
        
    }

    protected override void OnDestroy()
    {
        GetSingleton<UserListComponent>().allUser.Dispose();
    }
}