using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public class MoveByDirSystem : ComponentSystem
{
    static EntityQuery _query;
    protected override void OnCreate()
    {
        _query = GetEntityQuery(typeof(LMoveByDirComponent));
        base.OnCreate();
    }

    public static int Count => _query.CalculateEntityCount();

    protected override void OnUpdate()
    {
        var rvoEntity = GetSingletonEntity<RvoSimulatorComponet>();
        var rvoObj = EntityManager.GetComponentObject<RvoSimulatorComponet>(rvoEntity);
        var checkSum = this.GetSingletonObject<CheckSumComponet>().checkSum;

        Entities.ForEach((Entity entity, ref LTransformComponet tranCom, ref LRvoComponent rvoComponent, ref LMoveByDirComponent moveCom, ref MoveSpeedComponent speed)=>{
            rvoObj.SetAgentPrefVelocity(rvoComponent.rvoId, new RVO.Vector2(moveCom.dir.x, moveCom.dir.z) * speed.speed);
            tranCom.rotation = moveCom.dir;
            // checkSum.preRVO.CheckValue(entity, moveCom.dir.GetHashCode(), fpMath.asint(moveCom.dir.x),fpMath.asint(moveCom.dir.y),fpMath.asint(moveCom.dir.z));
        });

        rvoObj.DoStep();
        
        NativeArray<int> array = new NativeArray<int>(1024, Allocator.Temp);
        Entities.ForEach((Entity entity,  ref LTransformComponet tranCom, ref LRvoComponent rvoComponent, ref VLerpTransformCopmnet com)=>{
            var pos = rvoObj.GetAgentPosition(rvoComponent.rvoId);
            var forward = rvoObj.GetAgentDir(rvoComponent.rvoId);
            var neibourCount = rvoObj.GetAgentNeighbor(rvoComponent.rvoId, array);
            int hash = neibourCount;
            for(int i = 0; i < neibourCount; i++) hash = CheckSum.CombineHashCode(hash, array[i]);
            checkSum.preRVO.CheckValue(entity, hash);

            com.lerpTime = 0;

            // tranCom.rotation = new fp3(forward.x(), 0, forward.y());
            tranCom.position = new fp3(pos.x(), 0, pos.y());
        });
    }
}