// using Unity.Entities;
// using Unity.Mathematics;
// using UnityEngine;

// // 队友行为，模拟绕着玩家走的逻辑
// public class TeamMateAiInputSystem : ComponentSystem
// {
//     protected override void OnUpdate()
//     {   
//         if(!HasSingleton<ControllerHolder>()) return;

//         var controllerHolder = GetSingleton<ControllerHolder>();
//         var controllerEntity = controllerHolder.controller;
//         var binding = EntityManager.GetComponentData<GameObjectBindingComponent>(controllerEntity);
//         var pos = EntityManager.GetComponentData<LTransformComponet>(controllerEntity).position;

//         Entities.ForEach((Entity entity, ref LTransformComponet lTransformComponet)=>{
//             if(entity == controllerEntity) return;

//             var distance = math.distancesq(pos, lTransformComponet.position);
//             var posNext = lTransformComponet.position + math.mul(lTransformComponet.rotation, new float3(0, 0, 1));
//             var distanceSqNext = math.distancesq(posNext, pos);

//             var dir = lTransformComponet.rotation;

//             if(distanceSqNext > distance)
//             {
//                 // 转向
//                 dir = math.nlerp(lTransformComponet.rotation, quaternion.LookRotation(pos - lTransformComponet.position, new float3(0, 1, 0)), 0.5f);
//             }

//             var position += (Vector3)(Time.DeltaTime * dir * moveSpeedComponent.speed);

//             EntityManager.AddComponentData(EntityManager.CreateEntity(), new MessageUpdatePosEvent(){
//                 pos = tranCom.position,
//                 entity = controllerEntity
//             });
//         });

//         // var tranCom = binding.obj.transform;

//         // tranCom.rotation = math.nlerp(tranCom.rotation, quaternion.RotateY(angle), 0.05f);
//         // var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
//         // tranCom.position += (Vector3)(Time.DeltaTime * dir * moveSpeedComponent.speed);

//         // EntityManager.AddComponentData(EntityManager.CreateEntity(), new MessageUpdatePosEvent(){
//         //     pos = tranCom.position,
//         //     entity = controllerEntity
//         // });
//     }
// }