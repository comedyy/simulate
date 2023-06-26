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
        var checkSum = this.GetSingletonObject<CheckSumComponet>().checkSum;

        Entities.ForEach((Entity entity, ref LRvoComponent rvoComponent, ref LMoveByDirComponent moveCom, ref MoveSpeedComponent speed)=>{
            rvoObj.SetAgentPrefVelocity(rvoComponent.rvoId, new RVO.Vector2(moveCom.dir.x, moveCom.dir.z) * speed.speed);
            checkSum.preRVO.CheckValue(entity, moveCom.dir.GetHashCode(), fpMath.asint(moveCom.dir.x),fpMath.asint(moveCom.dir.y),fpMath.asint(moveCom.dir.z));
        });

        rvoObj.DoStep();
        
        Entities.ForEach((ref LTransformComponet tranCom, ref LRvoComponent rvoComponent, ref VLerpTransformCopmnet com)=>{
            var pos = rvoObj.GetAgentPosition(rvoComponent.rvoId);

            com.lerpTime = 0;

            var forward = fpMath.normalize(new fp3(pos.x(), 0, pos.y()) - tranCom.position);
            if(forward != fp3.zero)
            {
                tranCom.rotation = new fp3(forward.x, 0, forward.y);
            }
            tranCom.position = new fp3(pos.x(), 0, pos.y());
        });
    }
}