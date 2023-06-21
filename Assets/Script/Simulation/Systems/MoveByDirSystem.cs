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

        Entities.ForEach((ref LRvoComponent rvoComponent, ref LMoveByDirComponent moveCom, ref MoveSpeedComponent speed)=>{
            rvoObj.rvoSimulator.setAgentPrefVelocity(rvoComponent.rvoId, new RVO.Vector2(moveCom.dir.x, moveCom.dir.z) * speed.speed);
        });

        rvoObj.rvoSimulator.doStep();
        
        Entities.ForEach((ref LTransformComponet tranCom, ref LRvoComponent rvoComponent, ref VLerpTransformCopmnet com)=>{
            var pos = rvoObj.rvoSimulator.getAgentPosition(rvoComponent.rvoId);
            var forward = rvoObj.rvoSimulator.getAgentVelocity(rvoComponent.rvoId);

            com.lerpTime = 0;

            tranCom.rotation = new fp3(forward.x(), 0, forward.y());
            tranCom.position = new fp3(pos.x(), 0, pos.y());
        });
    }
}