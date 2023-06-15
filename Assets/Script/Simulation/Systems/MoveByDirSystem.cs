using Unity.Entities;
using Unity.Mathematics;


public class MoveByDirSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var rvoEntity = GetSingletonEntity<RvoSimulatorComponet>();
        var rvoObj = EntityManager.GetComponentObject<RvoSimulatorComponet>(rvoEntity);

        Entities.ForEach((ref LRvoComponent rvoComponent, ref LMoveByDirComponent moveCom)=>{
            var sin = math.sin(moveCom.dir);
            var cos = math.cos(moveCom.dir);
            rvoObj.rvoSimulator.setAgentPrefVelocity(rvoComponent.rvoId, new RVO.Vector2(cos, sin));
        });

        rvoObj.rvoSimulator.doStep();
        
        Entities.ForEach((ref LTransformComponet tranCom, ref LRvoComponent rvoComponent, ref VLerpTransformCopmnet com)=>{
            var pos = rvoObj.rvoSimulator.getAgentPosition(rvoComponent.rvoId);
            var forward = rvoObj.rvoSimulator.getAgentVelocity(rvoComponent.rvoId);

            com.lerpTime = 0;

            tranCom.rotation = quaternion.LookRotation(new float3(forward.x(), 0, forward.y()), new float3(0, 1, 0));
            tranCom.position = new float3(pos.x(), 0, pos.y());
        });
    }
}