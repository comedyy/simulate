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
        typeof(SizeComponent),
        typeof(HpComponent),
        typeof(AtkComponent),
        typeof(SkillComponent),

        typeof(GameObjectBindingComponent),
    };

    ComponentType[] archetypeUserComponents = new ComponentType[]{
        typeof(LTransformComponet), 
        // typeof(LMoveByPosComponent), 
        typeof(MoveSpeedComponent),
        typeof(VLerpTransformCopmnet), 
        typeof(SizeComponent),
        typeof(HpComponent),
        typeof(AtkComponent),
        typeof(SkillComponent),
        typeof(UserComponnet),
        typeof(UserAiComponent),

        typeof(UserMoveState),
        typeof(GameObjectBindingComponent)

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
        var random = this.GetSingletonObject<RandomComponent>().random;

        Entities.ForEach((Entity evEntity, ref SpawnEvent ev)=>{
            var entity = Entity.Null;
            var size = fp.one;
            
            if(ev.isUser)
            {
                entity = EntityManager.CreateEntity(_controllerArchetype);
                
                var userList = GetSingleton<UserListComponent>();
                userList.allUser.Add(entity);
                SetSingleton(userList);

                EntityManager.SetComponentData(entity, new SkillComponent(){
                     range = 5, interval = fp.Create(0, 3000)
                });

                EntityManager.SetComponentData(entity, new UserComponnet(){
                    id = ev.id
                });

                EntityManager.SetComponentData(entity, new UserAiComponent(){
                    offsetToController = new fp3(RandomUtils.Random(random, 5), 0, RandomUtils.Random(random, 5))
                });
                
                // var move = EntityManager.GetComponentData<LMoveByPosComponent>(entity);
                // move.pos = ev.position;
                // EntityManager.SetComponentData(entity, move);
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

                EntityManager.SetComponentData(entity, new LRvoComponent(){
                    rvoId = rvoObj.AddAgent(new RVO.Vector2(ev.position.x, ev.position.z), fp.Create(5), 3, fp.Create(0, 500), fp.Create(0, 500), fp.Create(0, 9000), 3, new RVO.Vector2(ev.dir.x, ev.dir.z), entity)
                });

                EntityManager.SetComponentData(entity, new SkillComponent(){
                    range = fp.one, interval = fp.Create(0, 3000)
                });
            }

            EntityManager.SetComponentData(entity, new HpComponent(){
                hp = ev.hp, hpMax = ev.hp
            });

            EntityManager.SetComponentData(entity, new AtkComponent(){
                atk = ev.atk
            });

            var transform = EntityManager.GetComponentData<LTransformComponet>(entity);
            transform.position = ev.position;
            transform.rotation = ev.dir;
            EntityManager.SetComponentData(entity, transform);

            EntityManager.SetComponentData<MoveSpeedComponent>(entity, new MoveSpeedComponent(){
                speed = fp.Create(7)
            });

            EntityManager.SetComponentData<SizeComponent>(entity, new SizeComponent(){
                size = size
            });

            var buffer = EntityManager.GetBuffer<SpawnEventComponent>(GetSingletonEntity<SpawnEventComponent>());
            buffer.Add(new SpawnEventComponent(){
                entity = entity,
                isUser = ev.isUser,
                id = ev.id
            });

            EntityManager.DestroyEntity(evEntity);
        });
    }
}