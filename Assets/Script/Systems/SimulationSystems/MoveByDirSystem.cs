using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class MoveByDirSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref LTransformComponet tranCom, ref LMoveByDirComponent moveCom)=>{
            tranCom.rotation = quaternion.AxisAngle(new float3(0, 1, 0), moveCom.dir);
            var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
            tranCom.position += Time.DeltaTime * dir;

            Debug.LogWarning("====== OnUpdate2 " + Time.ElapsedTime);

        });
    }
}