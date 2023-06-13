using Unity.Entities;
using Unity.Mathematics;


public class MoveByDirSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref LTransformComponet tranCom, ref LMoveByDirComponent moveCom)=>{
            tranCom.rotation = math.nlerp(tranCom.rotation, moveCom.dir, 0.3f);
            var dir = math.mul(tranCom.rotation, new float3(0, 0, 1));
            tranCom.position += Time.DeltaTime * dir;
        });
    }
}