using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class VLerpTransformSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var userEntity = GetSingleton<ControllerHolder>().controller;
        Entities.ForEach((Entity entity, GameObjectBindingComponent binding, ref VLerpTransformCopmnet lerp, ref LTransformComponet lTransformCom, ref LMoveByDirComponent dir)=>{
            if(entity == userEntity) return;

            lerp.lerpTime = math.min(lerp.lerpTime + Time.DeltaTime, 0.1f);
            var lerpValue = lerp.lerpTime / 0.1f;
            var pos = math.lerp(lerp.preLogicPos, lTransformCom.position, lerpValue);
            var rotation = math.nlerp(lerp.preLogicRatation, lTransformCom.rotation, lerpValue);
            // Debug.LogError(Time.DeltaTime + " " + lerp.lerpTime + " " + lerpValue + " " + pos + " " +  lerp.preLogicPos + " " + lTransformCom.position);

            binding.obj.transform.SetPositionAndRotation(pos, rotation);
        });
    }
}