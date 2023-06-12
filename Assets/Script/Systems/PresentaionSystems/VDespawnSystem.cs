using Unity.Entities;
using UnityEngine;

internal class VDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref VDespawnEvent ev)=>{
            var com = EntityManager.GetComponentObject<GameObjectBindingComponent>(ev.target);
            if(com != null && com.obj != null)
            {
                GameObject.Destroy(com.obj);
            }

            EntityManager.DestroyEntity(ev.target);
            EntityManager.DestroyEntity(entity);
        });
    }
}