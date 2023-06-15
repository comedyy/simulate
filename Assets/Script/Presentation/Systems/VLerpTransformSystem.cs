using Unity.Entities;
using Unity.Mathematics;


public class VLerpTransformSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var logicStep = GetSingleton<LogicTime>().deltaTime * 3; // 增加lerp的时间

        if(!HasSingleton<ControllerHolder>()) return;
        var userEntity = GetSingleton<ControllerHolder>().controller;
        Entities.ForEach((Entity entity, GameObjectBindingComponent binding, ref VLerpTransformCopmnet lerp, ref LTransformComponet lTransformCom)=>{
            if(binding.obj == null)
            {
                UnityEngine.Debug.LogError($"=== not found {entity}");
            }
            if(lerp.lerpTime == 0)
            {
                if(entity == userEntity)
                {
                    lerp.lerpBeginPos = binding.objFollow.transform.position;
                    lerp.lerpBeginRotation = binding.objFollow.transform.rotation;
                }
                else
                {
                    lerp.lerpBeginPos = binding.obj.transform.position;
                    lerp.lerpBeginRotation = binding.obj.transform.rotation;
                }
            }

            lerp.lerpTime = math.min(lerp.lerpTime + Time.DeltaTime, logicStep);
            var lerpValue = lerp.lerpTime / logicStep;
            var pos = math.lerp(lerp.lerpBeginPos, lTransformCom.position, lerpValue);
            var rotation = math.nlerp(lerp.lerpBeginRotation, lTransformCom.rotation, lerpValue);
            // Debug.LogError(Time.DeltaTime + " " + lerp.lerpTime + " " + lerpValue + " " + pos + " " +  lerp.preLogicPos + " " + lTransformCom.position);

            if(entity == userEntity)
            {
                binding.objFollow.transform.SetPositionAndRotation(pos, rotation);
            }
            else
            {
                if(binding.obj != null)
                {
                    binding.obj.transform.SetPositionAndRotation(pos, rotation);
                }
            }
        });
    }
}