using Unity.Entities;
using UnityEngine;

public class VDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref VDespawnEvent ev)=>{
            var entityBinding = GetSingletonEntity<BindingComponet>();
            var binding = EntityManager.GetComponentObject<BindingComponet>(entityBinding);
            if(binding.allObject.TryGetValue(ev.target, out var obj))
            {
                GameObject.Destroy(obj);
            }

            EntityManager.DestroyEntity(entity);
        });
    }
}