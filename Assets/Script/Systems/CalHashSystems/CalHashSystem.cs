using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class CalHashSystem : ComponentSystem
{
    EntityQuery _entityQuery;
    protected override void OnCreate()
    {
        base.OnCreate();

        _entityQuery = GetEntityQuery(typeof(LTransformComponet));
    }

    protected override void OnUpdate()
    {
    }
}