using Unity.Entities;
using Unity.Mathematics;
using System;
using Game.Battle.CommonLib;

public class InitiazationSystem : ComponentSystemBase
{
    public static fp logicFrameInterval;
    public static int userCount;

    public void Init(int worldId)
    {
        MSPathSystem.InitSystem(worldId);
        var component = new RvoSimulatorComponet(){
            id = worldId
        };
        component.setTimeStep(logicFrameInterval);
        EntityManager.AddComponentObject(EntityManager.CreateEntity(), component);
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        

        for(int i = 0; i < userCount; i++)
        {
            EntityManager.AddComponentData(EntityManager.CreateEntity(), new SpawnEvent()
            {
                position = new fp3(3, 0, 3),
                dir = new fp3(1, 0, 0),
                id = i + 1,
                isUser = true,
                atk = 10,
                hp = 200
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
        // var rvoSimulator = new RVO.Simulator();
        // rvoSimulator.setTimeStep(logicFrameInterval);
        // var comp = EntityManager.AddComponentObject(entity, new RvoSimulatorComponet(){
        //     rvoSimulator = rvoSimulator
        // });
      

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
        this.GetSingletonObject<RvoSimulatorComponet>().ShutDown();
    }
}