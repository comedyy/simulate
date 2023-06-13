using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CalHashSystem : ComponentSystem
{
    EntityQuery _entityQuery;
    public CheckSum _checkSum;
    protected override void OnCreate()
    {
        base.OnCreate();

        _entityQuery = GetEntityQuery(typeof(LTransformComponet));
    }

    protected override void OnUpdate()
    {
        if(_checkSum == null) return;

        int frame = GetSingleton<LogicTime>().frameCount;
        _checkSum.Reset(frame);
        
        Entities.ForEach((ref LTransformComponet ls)=>{
            _checkSum.CheckValue(ls.position.GetHashCode());
            _checkSum.CheckValue(ls.rotation.GetHashCode());
        });

        _checkSum.SaveCheckSum();
    }
}