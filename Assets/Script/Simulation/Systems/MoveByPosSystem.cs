using Unity.Entities;
using Unity.Mathematics;


// public class MoveByPosSystem : ComponentSystem
// {
//     protected override void OnUpdate()
//     {
//         Entities.ForEach((ref LTransformComponet tranCom, ref LMoveByPosComponent moveCom)=>{
//             var prePos = tranCom.position;
//             var diff = moveCom.pos - prePos;
//             if(diff.x != 0 || diff.y != 0 || diff.z != 0)
//             {
//                 var dir = math.normalize(diff);
//                 tranCom.rotation = quaternion.LookRotation(dir, new float3(0, 1, 0));
//                 tranCom.position = moveCom.pos;
//             }
//         });
//     }
// }