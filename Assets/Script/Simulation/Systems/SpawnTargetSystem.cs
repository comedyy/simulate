using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnTargetSystem : ComponentSystem
{
    ComponentType[] archetypeComponents = new ComponentType[]{
        typeof(LTransformComponet), 
        typeof(LMoveByDirComponent), 
        typeof(MoveSpeedComponent),
        typeof(MonsterAiComponent), 
        typeof(MonsterAutoDespawnComponent), 
        typeof(VLerpTransformCopmnet), 
        typeof(LRvoComponent),
        typeof(SizeComponent)
    };

    ComponentType[] archetypeUserComponents = new ComponentType[]{
        typeof(LTransformComponet), 
        typeof(LMoveByPosComponent), 
        typeof(MoveSpeedComponent),
        typeof(VLerpTransformCopmnet), 
        typeof(SizeComponent)

    };

    EntityArchetype _unitArchetype;
    EntityArchetype _controllerArchetype;
    protected override void OnCreate()
    {
        base.OnCreate();

        _unitArchetype = EntityManager.CreateArchetype(archetypeComponents);
        _controllerArchetype = EntityManager.CreateArchetype(archetypeUserComponents);
    }

    protected override void OnUpdate()
    {
        var rvoEntity = GetSingletonEntity<RvoSimulatorComponet>();
        var rvoObj = EntityManager.GetComponentObject<RvoSimulatorComponet>(rvoEntity);

        Entities.ForEach((Entity evEntity, ref SpawnEvent ev)=>{
            var entity = Entity.Null;
            var size = 1f;
            
            if(ev.isUser)
            {
                entity = EntityManager.CreateEntity(_controllerArchetype);
                
                var move = EntityManager.GetComponentData<LMoveByPosComponent>(entity);
                move.pos = ev.position;
                EntityManager.SetComponentData(entity, move);
            }
            else
            {
                entity = EntityManager.CreateEntity(_unitArchetype);
                EntityManager.SetComponentData(entity, new MonsterAiComponent(){
                    randomTurnInterval = ev.aiInterval
                });
                EntityManager.SetComponentData(entity, new MonsterAutoDespawnComponent(){
                    despawnTime = ev.despawnTime
                });
                EntityManager.SetComponentData(entity, new LMoveByDirComponent(){
                    dir = ev.dir 
                });


                var sin = math.sin(ev.dir);
                var cos = math.cos(ev.dir);
                EntityManager.SetComponentData(entity, new LRvoComponent(){
                    rvoId = rvoObj.rvoSimulator.addAgent(new RVO.Vector2(ev.position.x, ev.position.z), 1.5f, 3, 0.05f, 0.05f, size, 3, new RVO.Vector2(cos, sin))
                });
            }
            var transform = EntityManager.GetComponentData<LTransformComponet>(entity);
            transform.position = ev.position;
            transform.rotation = quaternion.RotateY(ev.dir);
            EntityManager.SetComponentData(entity, transform);

            EntityManager.SetComponentData<MoveSpeedComponent>(entity, new MoveSpeedComponent(){
                speed = 3f
            });

            EntityManager.SetComponentData<SizeComponent>(entity, new SizeComponent(){
                size = size
            });

            var vSpwanEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(vSpwanEntity, new VSpawnEvent(){
                target = entity,
                isUser = ev.isUser,
                isContorller = ev.isController
            });

            EntityManager.DestroyEntity(evEntity);
        });
    }
}