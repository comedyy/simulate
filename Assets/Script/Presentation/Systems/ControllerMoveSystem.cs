using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ControllerMoveSystem : ComponentSystem
{
    internal LocalFrame localServer;
    public bool userAutoMove;

    bool _isMoving = false;
    protected override void OnUpdate()
    {   
        var userEntity = GetSingleton<ControllerHolder>().controller;
        if(!EntityManager.Exists(userEntity)) return;

        var controllerHolder = GetSingleton<ControllerHolder>();
        var controllerEntity = controllerHolder.controller;

        if(!EntityManager.Exists(controllerEntity)) return;

        var binding = EntityManager.GetComponentData<GameObjectBindingComponent>(controllerEntity);
        if(binding.obj == null) return;
        int controllerId = EntityManager.GetComponentData<UserComponnet>(controllerEntity).id;

        var tranCom = binding.obj.transform;

        var moveSpeedComponent = EntityManager.GetComponentData<MoveSpeedComponent>(controllerEntity);
        if(!userAutoMove)   // 如果是在editor模式，只有id == 1的才是控制者，其他人都是ai
        {

            var angle = GetAngle();

            if(angle < 0) {

                if(_isMoving)
                {
                    localServer.SetData(new MessageItem(){
                        endMoving = true
                    });
                    _isMoving = false;
                    binding.animator.SetBool("Run", false);
                }
                
                return;
            }
            
            if(!_isMoving)
            {
                _isMoving = true;
                binding.animator.SetBool("Run", true);
            }
            
            tranCom.rotation = math.nlerp(tranCom.rotation, quaternion.RotateY(angle), 0.05f);
            var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
            tranCom.position += (Vector3)(UnityEngine.Time.deltaTime * dir * moveSpeedComponent.speed);


            var pos = tranCom.position;
            #if FIXED_POINT
            var targetPos = new int3((int)fp.UnsafeConvert(pos.x).rawValue,
                                        (int)fp.UnsafeConvert(pos.y).rawValue,
                                        (int)fp.UnsafeConvert(pos.z).rawValue);
            localServer.SetData(new MessageItem(){
                pos = targetPos
            });
            #else
            var targetPos = new int3((int)(pos.x * 100), (int)(pos.y * 100), (int)(pos.z * 100));
            localServer.SetData(new MessageItem(){
                pos = targetPos, id = controllerId
            });
            #endif

            
        }
        else    // ai
        {
            binding.animator.SetBool("Run", true);
            UpdateOtherUser(binding.obj, moveSpeedComponent.speed);
        }
    }

    Vector3 goToPos;
    Vector3? currentPos;
    private void UpdateOtherUser(GameObject controller, float speed)
    {
        if(!currentPos.HasValue || Vector3.Distance(currentPos.Value, goToPos) < 1)
        {
            var randomX = UnityEngine.Random.Range(-20, 20);
            var randomY = UnityEngine.Random.Range(-20, 20);
            goToPos = new Vector3(randomX, 0, randomY);   
        } 

        if(!currentPos.HasValue) currentPos = controller.transform.position;

        var dir = Vector3.Normalize(goToPos - currentPos.Value);
        currentPos = currentPos.Value + dir * UnityEngine.Time.deltaTime * speed;
        controller.transform.position = currentPos.Value;
        controller.transform.rotation = math.nlerp(controller.transform.rotation, quaternion.LookRotation(dir, new float3(0, 1, 0)), 0.05f);

        #if FIXED_POINT
        var targetPos = new int3((int)fp.UnsafeConvert(currentPos.Value.x).rawValue,
                                    (int)fp.UnsafeConvert(currentPos.Value.y).rawValue,
                                    (int)fp.UnsafeConvert(currentPos.Value.z).rawValue);
        localServer.SetData(new MessageItem(){
            pos = targetPos
        });
        #else
        var targetPos = new int3((int)(pos.x * 100), (int)(pos.y * 100), (int)(pos.z * 100));
        localServer.SetData(new MessageItem(){
            pos = targetPos, id = controllerId
        });
        #endif
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

        if(SimpleJoystick.Instance.GetDir(out var dir))
        {
            angle = math.radians(dir);
        }

        return angle;
    }
}