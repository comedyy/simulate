using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class VLerpTransformSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<ControllerTag>().ForEach((GameObjectBindingComponent binding, ref VLerpTransformCopmnet lerp, ref LTransformComponet lTransformCom, ref LMoveByDirComponent dir)=>{
            lerp.lerpTime = math.min(lerp.lerpTime + Time.DeltaTime, 0.1f);
            var lerpValue = lerp.lerpTime / 0.1f;
            var pos = math.lerp(lerp.preLogicPos, lTransformCom.position, lerpValue);
            var rotation = math.nlerp(lerp.preLogicRatation, lTransformCom.rotation, lerpValue);

            binding.obj.transform.SetPositionAndRotation(pos, rotation);
        });
    }
}