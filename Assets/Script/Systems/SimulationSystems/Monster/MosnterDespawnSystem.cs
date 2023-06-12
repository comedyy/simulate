using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class MosnterDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref MonsterAutoDespawnComponent despawnComponent)=>{
            if(Time.ElapsedTime < despawnComponent.despawnTime)
            {
                return;
            }

            var despawnEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(despawnEntity, new VDespawnEvent(){
                target = entity
            });

            var spawnCountCom = GetSingleton<SpawnMonsterComponent>();
            spawnCountCom.currentCount--;
            SetSingleton(spawnCountCom);
        });
    }
}