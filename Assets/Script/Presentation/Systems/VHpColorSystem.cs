using System;
using Game;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
            DamageNumManager.Instance.ShowDamage(ev.value, gameObj.transform.position);

            // if(gameObj == null)
            // {
            //     UnityEngine.Debug.LogError($"=== not found {target}");
            // }
            // var renderer = gameObj.GetComponentInChildren<Renderer>();
            // var hpComponent = EntityManager.GetComponentData<HpComponent>(target);
            // renderer.material.color = CalHpColor(hpComponent.hp, hpComponent.hpMax);
        };
    }

    private static Color CalHpColor(int hp, int hpMax)
    {
        return Color.Lerp(Color.green, Color.red, 1 - hp * 1.0f / hpMax);
    }
}