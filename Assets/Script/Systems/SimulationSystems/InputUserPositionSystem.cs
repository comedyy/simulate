using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class InputUserPositionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var userEntity = GetSingleton<ControllerHolder>().controller;
        var binding = EntityManager.GetComponentData<GameObjectBindingComponent>(userEntity);
        if(binding != null && binding.obj != null) 
        {
            EntityManager.SetComponentData(userEntity, new LMoveByPosComponent(){
                pos = binding.obj.transform.position
            });
        }
    }
}