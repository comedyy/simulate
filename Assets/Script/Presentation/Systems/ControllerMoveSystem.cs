using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ControllerMoveSystem : ComponentSystem
{
    internal LocalFrame localServer;

    protected override void OnUpdate()
    {   
        var userEntity = GetSingleton<ControllerHolder>().controller;
        if(!EntityManager.Exists(userEntity)) return;

        var controllerHolder = GetSingleton<ControllerHolder>();
        var controllerEntity = controllerHolder.controller;
        var binding = EntityManager.GetComponentData<GameObjectBindingComponent>(controllerEntity);
        if(binding.obj == null) return;

        var moveSpeedComponent = EntityManager.GetComponentData<MoveSpeedComponent>(controllerEntity);

        var angle = GetAngle();

        if(angle < 0) return;

        var tranCom = binding.obj.transform;

        tranCom.rotation = math.nlerp(tranCom.rotation, quaternion.RotateY(angle), 0.05f);
        var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
        tranCom.position += (Vector3)(Time.DeltaTime * dir * moveSpeedComponent.speed);

        localServer.SetData(new MessageItem(){
            pos = tranCom.position, id = EntityManager.GetComponentData<UserComponnet>(controllerEntity).id
        });

        // simulate other role behavior
        
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