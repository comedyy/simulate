using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class CalHashSystem : ComponentSystem
{
    EntityQuery _entityQuery;
    CheckSum _checkSum;
    protected override void OnCreate()
    {
        base.OnCreate();

        _entityQuery = GetEntityQuery(typeof(LTransformComponet));
        _checkSum = new CheckSum();
    }

    protected override void OnUpdate()
    {
        int frame = GetSingleton<LogicTime>().frameCount;
        _checkSum.Reset(frame);
        
        Entities.ForEach((ref LTransformComponet ls)=>{
            _checkSum.CheckValue(ls.position.GetHashCode());
            _checkSum.CheckValue(ls.rotation.GetHashCode());
        });

        _checkSum.SaveCheckSum();
        Debug.LogError(_checkSum.GetCheckSum());
    }
}