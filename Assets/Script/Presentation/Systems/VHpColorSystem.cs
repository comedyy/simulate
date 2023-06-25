using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisableAutoCreation]
public class VHpColorSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var buffer = EntityManager.GetBuffer<VHurtComponent>(GetSingletonEntity<VHurtComponent>());
        for(int i = 0; i < buffer.Length; i++)
        {
            var ev = buffer[i];
            var target = ev.target;
            if(!EntityManager.Exists(target)) continue;

            var gameObj = EntityManager.GetComponentObject<GameObjectBindingComponent>(target).obj;
            if(gameObj == null)
            {
                UnityEngine.Debug.LogError($"=== not found {target}");
            }
            var renderer = gameObj.GetComponentInChildren<Renderer>();
            renderer.material.color = CalHpColor(EntityManager.GetComponentData<HpComponent>(target).hp);
        };
    }

    private static Color CalHpColor(int hp)
    {
        return Color.Lerp(Color.green, Color.red, 1 - hp / 100f);
    }
}