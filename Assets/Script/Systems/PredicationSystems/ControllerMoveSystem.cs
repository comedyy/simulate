using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class ControllerMoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {   
        if(!HasSingleton<ControllerHolder>()) return;

        var controllerHolder = GetSingleton<ControllerHolder>();
        var controllerEntity = controllerHolder.controller;
        var binding = EntityManager.GetComponentData<GameObjectBindingComponent>(controllerEntity);
        var moveSpeedComponent = EntityManager.GetComponentData<MoveSpeedComponent>(controllerEntity);

        var angle = GetAngle();

        if(angle < 0) return;

        var tranCom = binding.obj.transform;

        tranCom.rotation = math.nlerp(tranCom.rotation, quaternion.RotateY(angle), 0.05f);
        var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
        tranCom.position += (Vector3)(Time.DeltaTime * dir * moveSpeedComponent.speed);

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