using System.Collections;
using TextureRendering;
using Unity.Entities;
using UnityEngine;

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
                Main.Instance.StartCoroutine(DieFunc(obj));
            }
        }
    }

    IEnumerator DieFunc(GameObject obj)
    {
        var param = obj.GetComponent<DissolveParams>();
        var material = new Material(param.material);
        obj.GetComponentInChildren<Renderer>().material = material;
        obj.GetComponent<Animator>().Play("die");
        var t = 0f;
        while(t < param.dissolveTime)
        {
            var percent = t / param.dissolveTime;
            t+= Time.DeltaTime;
            material.SetFloat("_Clip", percent);
            yield return null;
        }
        
        GameObject.Destroy(obj);
    }

}