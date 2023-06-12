using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class ControllerMoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((GameObjectBindingComponent binding, ref ControllerTag tag, ref MoveSpeedComponent moveSpeedComponent)=>{
            var angle = GetAngle();

            if(angle < 0) return;

            var tranCom = binding.obj.transform;

            tranCom.rotation = quaternion.AxisAngle(new float3(0, 1, 0), angle);
            var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
            tranCom.position += (Vector3)(Time.DeltaTime * dir * moveSpeedComponent.speed);

            // create send msg
        });
    }

    static float GetAngle()
    {
        float angle = -1;
        if(Input.GetKey(KeyCode.W)){
            angle = 0;            
        }
        else if(Input.GetKey(KeyCode.A))
        {
            angle = math.radians(270); 
        }
        else if(Input.GetKey(KeyCode.D))
        {
            angle = math.radians(90);
        }
        else if(Input.GetKey(KeyCode.S))
        {
            angle = math.radians(180);
        }

        return angle;
    }
}