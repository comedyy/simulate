using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MosnterDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var escaped = GetSingleton<LogicTime>().escaped;

        Entities.ForEach((Entity entity, ref MonsterAutoDespawnComponent despawnComponent)=>{
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

            var spawnCountCom = GetSingleton<SpawnMonsterComponent>();
            spawnCountCom.currentCount--;
            SetSingleton(spawnCountCom);
        });
    }
}