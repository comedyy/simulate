using Unity.Entities;
using Unity.Mathematics;

public struct MessageUpdatePosEvent : IComponentData
{
    public Entity entity;
    public float3 pos;
}