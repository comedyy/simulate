using System;
using System.Collections.Generic;
using Game.Battle.CommonLib;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


public class HurtSystem : ComponentSystemBase
{
    public override void Update()
    {
        OnUpdate();
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected void OnUpdate()
    {
        var buffer = EntityManager.GetBuffer<HurtComponent>(GetSingletonEntity<HurtComponent>());
        var bufferVHurt = EntityManager.GetBuffer<VHurtComponent>(GetSingletonEntity<VHurtComponent>());

        NativeList<Entity> _lst = new NativeList<Entity>(Allocator.TempJob);
        HurtJob hurtJob = new HurtJob(){
            buffer = buffer,
            vDesposeBuffer = EntityManager.GetBuffer<DeSpawnEventComponent>(GetSingletonEntity<DeSpawnEventComponent>()),
            bufferVHurt = bufferVHurt,
            EntityManager = EntityManager,
            _lst = _lst
        };
        hurtJob.Run();


        var buffer1 = EntityManager.GetBuffer<HurtComponent>(GetSingletonEntity<HurtComponent>());
        buffer1.Clear();

        var processDeadJob = new ProcessDeadJob(){
            entityManager = EntityManager,
            entityUserSington = GetSingletonEntity<UserListComponent>(),
            _lst = _lst
        };
        processDeadJob.Run();
        _lst.Dispose();
    }

    [BurstCompile]
    struct ProcessDeadJob : IJob
    {
        public EntityManager entityManager;
        public Entity entityUserSington;
        internal NativeList<Entity> _lst;

        public void Execute()
        {
            for(int i = 0; i < _lst.Length; i++)
            {
                var entity = _lst[i];
                if(entityManager.HasComponent<LRvoComponent>(entity))
                {
                    var rvoComponent = entityManager.GetComponentData<LRvoComponent>(entity);
                    MSPathSystem.RemoveAgent(rvoComponent.idWorld, rvoComponent.rvoId);
                }
                else
                {
                    var listUser = entityManager.GetComponentData<UserListComponent>(entityUserSington).allUser;
                    for(int j = 0; j < listUser.length; j++)
                    {
                        if(listUser[j] == entity)
                        {
                            listUser.RemoveAt(j);
                            entityManager.SetComponentData(entityUserSington, new UserListComponent(){allUser = listUser});
                            break;
                        }
                    }
                }
                
                entityManager.DestroyEntity(_lst[i]);
            }
        }
    }

    [BurstCompile]
    struct HurtJob : IJob
    {
        [ReadOnly]
        public DynamicBuffer<HurtComponent> buffer;
        public DynamicBuffer<VHurtComponent> bufferVHurt;
        public EntityManager EntityManager;
        internal DynamicBuffer<DeSpawnEventComponent> vDesposeBuffer;
        internal NativeList<Entity> _lst;

        public void Execute()
        {
            for(int i = 0; i < buffer.Length; i++)
            {
                var ev = buffer[i];
                var entity = ev.target;
                if(!EntityManager.Exists(entity))
                {
                    continue;
                }

                var hpComponent = EntityManager.GetComponentData<HpComponent>(entity);

                if(hpComponent.hp <= 0) continue;

                hpComponent.hp -= ev.value;
                EntityManager.SetComponentData(entity, hpComponent);
                bufferVHurt.Add(new VHurtComponent(){
                    target = entity, value = ev.value
                });

                if(hpComponent.hp <= 0)
                {
                    _lst.Add(entity);
                    vDesposeBuffer.Add(new DeSpawnEventComponent(){entity = entity});
                }
            }
        }
    }
}