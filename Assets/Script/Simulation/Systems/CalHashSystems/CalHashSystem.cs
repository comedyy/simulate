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
        // checkSum.Reset(0);
        
        Entities.ForEach((Entity entity, ref LTransformComponet ls)=>{
            checkSum.CheckValue(entity, ls.position.GetHashCode());
            checkSum.CheckValue(entity, ls.rotation.GetHashCode());
        });

        checkSum.SaveCheckSum();
    }
}