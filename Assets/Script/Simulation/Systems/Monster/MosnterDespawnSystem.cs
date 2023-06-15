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

            var buffer = EntityManager.GetBuffer<DeSpawnEventComponent>(GetSingletonEntity<DeSpawnEventComponent>());
            buffer.Add(new DeSpawnEventComponent(){
                entity = entity,
            });

            rvoObj.rvoSimulator.removeAgent(rvoComponent.rvoId);

            EntityManager.DestroyEntity(entity);

            var spawnCountCom = GetSingleton<SpawnMonsterComponent>();
            spawnCountCom.currentCount--;
            SetSingleton(spawnCountCom);
        });
    }
}