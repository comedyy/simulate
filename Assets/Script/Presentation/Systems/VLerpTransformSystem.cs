using Unity.Entities;
using Unity.Mathematics;


public class VLerpTransformSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var logicStep = GetSingleton<LogicTime>().deltaTime * 1; // 增加lerp的时间

        if(!HasSingleton<ControllerHolder>()) return;
        var userEntity = GetSingleton<ControllerHolder>().controller;
        Entities.ForEach((Entity entity, GameObjectBindingComponent binding, ref VLerpTransformCopmnet lerp, ref LTransformComponet lTransformCom)=>{
            if(binding.obj == null)
            {
                UnityEngine.Debug.LogError($"=== not found {entity}");
            }

            var obj = binding.objFollow == null ? binding.obj : binding.objFollow;
            if(lerp.lerpTime == 0)
            {
                lerp.lerpBeginPos = obj.transform.position;
                lerp.lerpBeginRotation = obj.transform.rotation;
            }

            lerp.lerpTime = math.min(lerp.lerpTime + Time.DeltaTime, logicStep);
            var lerpValue = lerp.lerpTime / logicStep;
            var pos = math.lerp(lerp.lerpBeginPos, lTransformCom.position, lerpValue);
            var rotation = math.nlerp(lerp.lerpBeginRotation, quaternion.LookRotation(lTransformCom.rotation, new float3(0, 1, 0)) , lerpValue);
            // Debug.LogError(Time.DeltaTime + " " + lerp.lerpTime + " " + lerpValue + " " + pos + " " +  lerp.preLogicPos + " " + lTransformCom.position);

            obj.transform.SetPositionAndRotation(pos, rotation);
        });

        // var listUser = GetSingleton<UserListComponent>().allUser;
        // for(int i = 0; i < listUser.length; i++)
        // {
        //     var animator = EntityManager.GetComponentData<GameObjectBindingComponent>(listUser[i]).animator;
        //     var lerpTime = EntityManager.GetComponentData<VLerpTransformCopmnet>(listUser[i]).lerpTime;
        //     var isMoving = lerpTime < logicStep * 2;
        //     if(animator != null) animator.SetBool("Run", isMoving);
        // }
    }
}