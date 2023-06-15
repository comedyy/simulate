using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CalHashSystem : ComponentSystem
{
    EntityQuery _entityQuery;
    protected override void OnCreate()
    {
        base.OnCreate();

        _entityQuery = GetEntityQuery(typeof(LTransformComponet));
    }

    protected override void OnUpdate()
    {
        var checkSum = this.GetSingletonObject<CheckSumComponet>().checkSum;

        int frame = GetSingleton<LogicTime>().frameCount;
        
        Entities.ForEach((Entity entity, ref LTransformComponet ls)=>{
            checkSum.positionChecksum.CheckValue(entity, ls.position.GetHashCode());
            checkSum.positionChecksum.CheckValue(entity, ls.rotation.GetHashCode());
        });

        checkSum.positionChecksum.SaveCheckSum();

        Entities.ForEach((Entity entity, ref HpComponent ls)=>{
            checkSum.hpCheckSum.CheckValue(entity, ls.hp.GetHashCode());
        });

        checkSum.hpCheckSum.SaveCheckSum();

        checkSum.targetFindCheckSum.SaveCheckSum();
    }
}