using Unity.Entities;

public struct SpawnEventComponent : IBufferElementData
{
    public Entity entity;
    public bool isUser;
    public int id;
}