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
using UnityEngine.Profiling;

struct TempUserPos
{
    public Entity entity;
    public fp3 pos;
    public fp radius;
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
        Profiler.BeginSample("EnemyHurtUser");
        EnemyHurtUser(logic.escaped, inputDeps);
        Profiler.EndSample();
        Profiler.BeginSample("UserHurtEnemy");
        UserHurtEnemy(logic.escaped, this.GetSingletonObject<RvoSimulatorComponet>().id);
        Profiler.EndSample();

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
        Profiler.BeginSample("EnemyHurtUser1");
        var listUser = GetSingleton<UserListComponent>().allUser;
        NativeArray<TempUserPos> userPosList = new NativeArray<TempUserPos>(listUser.length, Allocator.TempJob);
        for(int i = 0; i < listUser.length; i++)
        {
            var item = listUser[i];
            userPosList[i] = new TempUserPos(){entity = item, 
            pos = EntityManager.GetComponentData<LTransformComponet>(item).position,
            radius = EntityManager.GetComponentData<SizeComponent>(item).size,
            };
        }
        Profiler.EndSample();

        Profiler.BeginSample("EnemyHurtUser2");
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
        Profiler.EndSample();

        Profiler.BeginSample("EnemyHurtUser3");
        ecb.Playback(EntityManager);
        Profiler.EndSample();
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
                    var radius = item.radius + skill.range;

                    if(fpMath.distancesq(myPos, pos) > radius * radius)
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