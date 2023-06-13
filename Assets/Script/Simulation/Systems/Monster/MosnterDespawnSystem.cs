using Unity.Entities;
using Unity.Mathematics;


public class MosnterDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref MonsterAutoDespawnComponent despawnComponent)=>{
            if(Time.ElapsedTime < despawnComponent.despawnTime)
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