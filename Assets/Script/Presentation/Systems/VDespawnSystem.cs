using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class VDespawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var buffer = EntityManager.GetBuffer<DeSpawnEventComponent>(GetSingletonEntity<DeSpawnEventComponent>());
        for(int i = 0; i < buffer.Length; i++)
        {
            var ev = buffer[i];

            var entityBinding = GetSingletonEntity<BindingComponet>();
            var binding = EntityManager.GetComponentObject<BindingComponet>(entityBinding);
            if(binding.allObject.TryGetValue(ev.entity, out var obj))
            {
                GameObject.Destroy(obj);
            }
        }
    }
}