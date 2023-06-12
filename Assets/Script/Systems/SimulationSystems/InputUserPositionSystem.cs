using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class InputUserPositionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((GameObjectBindingComponent binding, ref ControllerTag tag, ref LMoveByPosComponent lMoveByPosComponent)=>{
            if(binding != null && binding.obj != null) 
            {
                lMoveByPosComponent.pos = binding.obj.transform.position;
            }
        });
    }
}