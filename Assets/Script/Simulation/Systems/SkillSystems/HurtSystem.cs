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
    EntityQuery _queryDead;

    public override void Update()
    {
        OnUpdate();
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        _queryDead = GetEntityQuery(ComponentType.ReadOnly<Dead>());
    }

    protected void OnUpdate()
    {
        var rvoObj = this.GetSingletonObject<RvoSimulatorComponet>();
        var buffer = EntityManager.GetBuffer<HurtComponent>(GetSingletonEntity<HurtComponent>());
        var bufferVHurt = EntityManager.GetBuffer<VHurtComponent>(GetSingletonEntity<VHurtComponent>());

        NativeList<Entity> _lst = new NativeList<Entity>(Allocator.TempJob);
        HurtJob hurtJob = new HurtJob(){
            buffer = buffer,
            bufferVHurt = bufferVHurt,
            EntityManager = EntityManager,
            _lst = _lst
        };
        hurtJob.Run();

        foreach(var entity in _lst)
        {
            EntityManager.AddComponent<Dead>(entity);
        }
        _lst.Dispose();

        var buffer1 = EntityManager.GetBuffer<HurtComponent>(GetSingletonEntity<HurtComponent>());
        buffer1.Clear();

        var processDeadJob = new ProcessDeadJob(){
            entityManager = EntityManager,
            vDesposeBuffer = EntityManager.GetBuffer<DeSpawnEventComponent>(GetSingletonEntity<DeSpawnEventComponent>()),
            entityTypeChunk = GetArchetypeChunkEntityType(),
            rvoComponentChunkType = GetArchetypeChunkComponentType<LRvoComponent>(true),
            entityUserSington = GetSingletonEntity<UserListComponent>()
        };
        processDeadJob.Run(_queryDead);
    }

    [BurstCompile]
    struct ProcessDeadJob : IJobChunk
    {
        public EntityManager entityManager;
        internal DynamicBuffer<DeSpawnEventComponent> vDesposeBuffer;
        [ReadOnly]
        internal ArchetypeChunkComponentType<LRvoComponent> rvoComponentChunkType;
        [ReadOnly]
        internal ArchetypeChunkEntityType entityTypeChunk;
        public Entity entityUserSington;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var entities = chunk.GetNativeArray(entityTypeChunk);
            for(int i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
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
                
                vDesposeBuffer.Add(new DeSpawnEventComponent(){entity = entities[i]});
                entityManager.DestroyEntity(entities[i]);
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
                }
            }
        }
    }
}