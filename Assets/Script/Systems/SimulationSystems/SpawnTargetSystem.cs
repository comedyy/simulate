using System;
using Unity.Entities;

internal class SpawnTargetSystem : ComponentSystem
{
    ComponentType[] archetypeComponents = new ComponentType[]{
        typeof(LTransformComponet), 
        typeof(LMoveByDirComponent), 
        typeof(MoveSpeedComponent),
        typeof(VLerpTransformCopmnet), 
        typeof(GameObjectBindingComponent), 
    };

    ComponentType[] archetypeUserComponents = new ComponentType[]{
        typeof(LTransformComponet), 
        typeof(LMoveByPosComponent), 
        typeof(MoveSpeedComponent),
        typeof(VLerpTransformCopmnet), 
        typeof(GameObjectBindingComponent), 
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
        Entities.ForEach((Entity evEntity, ref SpawnEvent ev)=>{
            var entity = Entity.Null;
            
            if(ev.isUser)
            {
                entity = EntityManager.CreateEntity(_controllerArchetype);
            }
            else
            {
                entity = EntityManager.CreateEntity(_unitArchetype);
            }
            var transform = EntityManager.GetComponentData<LTransformComponet>(entity);
            transform.position = ev.position;
            transform.rotation = ev.dir;

            EntityManager.SetComponentData<MoveSpeedComponent>(entity, new MoveSpeedComponent(){
                speed = 3f
            });

            if(ev.isUser)
            {
                EntityManager.AddComponent<ControllerTag>(entity);
            }

            var vSpwanEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(vSpwanEntity, new VSpawnEvent(){
                target = entity
            });

            EntityManager.DestroyEntity(evEntity);
        });
    }
}