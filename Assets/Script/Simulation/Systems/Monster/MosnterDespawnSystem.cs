using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MosnterDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var escaped = GetSingleton<LogicTime>().escaped;

        var rvoEntity = GetSingletonEntity<RvoSimulatorComponet>();
        var rvoObj = EntityManager.GetComponentObject<RvoSimulatorComponet>(rvoEntity);

        Entities.ForEach((Entity entity, ref MonsterAutoDespawnComponent despawnComponent, ref LRvoComponent rvoComponent)=>{
            if(escaped < despawnComponent.despawnTime)
            {
                return;
            }

#if !ONLY_LOGIC
            var despawnEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(despawnEntity, new VDespawnEvent(){
                target = entity
            });
#endif
            rvoObj.rvoSimulator.removeAgent(rvoComponent.rvoId);

            EntityManager.DestroyEntity(entity);

            var spawnCountCom = GetSingleton<SpawnMonsterComponent>();
            spawnCountCom.currentCount--;
            SetSingleton(spawnCountCom);
        });
    }
}