using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class MoveByPosSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref LTransformComponet tranCom, ref LMoveByPosComponent moveCom)=>{
            var prePos = tranCom.position;
            tranCom.rotation = quaternion.LookRotation(math.normalize(moveCom.pos - prePos), new float3(0, 1, 0));
            tranCom.position = moveCom.pos;
        });
    }
}