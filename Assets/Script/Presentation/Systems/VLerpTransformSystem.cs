using Unity.Entities;
using Unity.Mathematics;


public class VLerpTransformSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var userEntity = GetSingleton<ControllerHolder>().controller;
        Entities.ForEach((Entity entity, GameObjectBindingComponent binding, ref VLerpTransformCopmnet lerp, ref LTransformComponet lTransformCom)=>{
            lerp.lerpTime = math.min(lerp.lerpTime + Time.DeltaTime, 0.1f);
            var lerpValue = lerp.lerpTime / 0.1f;
            var pos = math.lerp(lerp.preLogicPos, lTransformCom.position, lerpValue);
            var rotation = math.nlerp(lerp.preLogicRatation, lTransformCom.rotation, lerpValue);
            // Debug.LogError(Time.DeltaTime + " " + lerp.lerpTime + " " + lerpValue + " " + pos + " " +  lerp.preLogicPos + " " + lTransformCom.position);

            if(entity == userEntity)
            {
                binding.objFollow.transform.SetPositionAndRotation(pos, rotation);
            }
            else
            {
                binding.obj.transform.SetPositionAndRotation(pos, rotation);
            }
        });
    }
}