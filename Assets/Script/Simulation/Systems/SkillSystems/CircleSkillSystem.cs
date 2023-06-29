using System;
using System.Collections.Generic;
using Game.Battle.CommonLib;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

struct TempUserPos
{
    public Entity entity;
    public fp3 pos;
}

public class CircleSkillSystem : JobComponentSystem
{
    EntityQuery _queryAllEnemy;
    EntityQuery _queryAllUser;
    protected override void OnCreate()
    {
        base.OnCreate();
        EntityQueryDesc desc = new EntityQueryDesc(){
            All = new ComponentType[]{
                ComponentType.ReadOnly<LTransformComponet>(),
                ComponentType.ReadOnly<AtkComponent>(),
                typeof(SkillComponent)
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<UserComponnet>()
            }
        };
        _queryAllEnemy = GetEntityQuery(desc);

        _queryAllUser = GetEntityQuery(
            ComponentType.ReadOnly<LTransformComponet>(),
            ComponentType.ReadOnly<AtkComponent>(),
            typeof(SkillComponent),
            ComponentType.ReadOnly<UserComponnet>()
            );
    }

    List<Entity> tempEntities = new List<Entity>();
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        LogicTime logic = GetSingleton<LogicTime>();
        EnemyHurtUser(logic.escaped, inputDeps);
        UserHurtEnemy(logic.escaped, this.GetSingletonObject<RvoSimulatorComponet>().id);

        return default;
    }

    private void UserHurtEnemy(fp escaped, int worldId)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var hurtEventEntity = GetSingletonEntity<HurtComponent>();
        var userHurtEnemy = new UseHurtEnemyJob(){
            ecbParallel = ecb.ToConcurrent(),
            eventEntity = hurtEventEntity,
            transformComponentChunkType = GetArchetypeChunkComponentType<LTransformComponet>(true),
            atkComponentChunkType = GetArchetypeChunkComponentType<AtkComponent>(true),
            skillComponentChunkType = GetArchetypeChunkComponentType<SkillComponent>(),
            logicEscaped = escaped,
            worldId = worldId
        };
        userHurtEnemy.Schedule(_queryAllUser, default).Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    private void EnemyHurtUser(fp logicTime, JobHandle dep)
    {
        var listUser = GetSingleton<UserListComponent>().allUser;
        NativeArray<TempUserPos> userPosList = new NativeArray<TempUserPos>(listUser.length, Allocator.TempJob);
        for(int i = 0; i < listUser.length; i++)
        {
            var item = listUser[i];
            userPosList[i] = new TempUserPos(){entity = item, pos = EntityManager.GetComponentData<LTransformComponet>(item).position};
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var hurtEventEntity = GetSingletonEntity<HurtComponent>();
        var enemyHurtUser = new EnemyHurtUserJob(){
            ecbParallel = ecb.ToConcurrent(),
            eventEntity = hurtEventEntity,
            userList = userPosList,
            transformComponentChunkType = GetArchetypeChunkComponentType<LTransformComponet>(true),
            atkComponentChunkType = GetArchetypeChunkComponentType<AtkComponent>(true),
            skillComponentChunkType = GetArchetypeChunkComponentType<SkillComponent>(),
            logicEscaped = logicTime
        };
        enemyHurtUser.Schedule(_queryAllEnemy, dep).Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();

        userPosList.Dispose();
    }

    [BurstCompile]
    struct EnemyHurtUserJob : IJobChunk
    {
        internal EntityCommandBuffer.Concurrent ecbParallel;
        [ReadOnly]
        public Entity eventEntity;
        [ReadOnly]
        public NativeArray<TempUserPos> userList;
        [ReadOnly]
        internal ArchetypeChunkComponentType<LTransformComponet> transformComponentChunkType;
        [ReadOnly]
        internal ArchetypeChunkComponentType<AtkComponent> atkComponentChunkType;
        internal ArchetypeChunkComponentType<SkillComponent> skillComponentChunkType;
        internal fp logicEscaped;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var skills = chunk.GetNativeArray(skillComponentChunkType);
            var atks = chunk.GetNativeArray(atkComponentChunkType);
            var trans = chunk.GetNativeArray(transformComponentChunkType);

            for(int j = 0; j < chunk.Count; j++)
            {
                var skill = skills[j];
                if(logicEscaped - skill.preHurtTime < skill.interval) return;

                skill.preHurtTime = logicEscaped;
                skills[j] = skill;
                var atkValue = atks[j].atk;

                var myPos = trans[j].position;
                for(int i = 0; i < userList.Length; i++)
                {
                    var item = userList[i];
                    var pos = item.pos;
                    if(fpMath.distancesq(myPos, pos) > skill.range * skill.range)
                    {
                        continue;
                    }

                    ecbParallel.AppendToBuffer(chunkIndex, eventEntity, new HurtComponent(){
                        target = item.entity,
                        value =  atkValue
                    });
                }
            }
        }
    }

    [BurstCompile]
    struct UseHurtEnemyJob : IJobChunk
    {
        internal EntityCommandBuffer.Concurrent ecbParallel;
        [ReadOnly]
        public Entity eventEntity;
        [ReadOnly]
        internal ArchetypeChunkComponentType<LTransformComponet> transformComponentChunkType;
        [ReadOnly]
        internal ArchetypeChunkComponentType<AtkComponent> atkComponentChunkType;
        internal ArchetypeChunkComponentType<SkillComponent> skillComponentChunkType;
        internal fp logicEscaped;
        public int worldId;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var skills = chunk.GetNativeArray(skillComponentChunkType);
            var atks = chunk.GetNativeArray(atkComponentChunkType);
            var trans = chunk.GetNativeArray(transformComponentChunkType);
            var userList = new NativeArray<int>(1024, Allocator.Temp);

            for(int j = 0; j < chunk.Count; j++)
            {
                var skill = skills[j];
                if(logicEscaped - skill.preHurtTime < skill.interval) return;

                skill.preHurtTime = logicEscaped;
                skills[j] = skill;
                var atkValue = atks[j].atk;

                var myPos = trans[j].position;
                var count = MSPathSystem.GetNearByAgents(worldId, myPos.x.To32Fp, myPos.z.To32Fp, userList, skill.range.To32Fp);
                for(int i = 0; i < count; i++)
                {
                    var item = userList[i];
                    ecbParallel.AppendToBuffer(chunkIndex, eventEntity, new HurtComponent(){
                        target = new Entity(){Index = item >> 16, Version = item & 0xFFFF},
                        value =  atkValue
                    });
                }
            }
        }
    }
}