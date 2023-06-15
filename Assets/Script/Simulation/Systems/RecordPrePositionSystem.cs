using Unity.Entities;
using Unity.Mathematics;


// public class RecordPrePositionSystem : ComponentSystem
// {
//     protected override void OnUpdate()
//     {
//         Entities.ForEach((ref LTransformComponet tranCom, ref VLerpTransformCopmnet lerpComp)=>{
//             lerpComp.lerpTime = 0;
//             lerpComp.preLogicPos = tranCom.position;
//             lerpComp.preLogicRatation = tranCom.rotation;
//         });
//     }
//}