using Unity.Entities;

public struct SpawnEventComponent : IBufferElementData
{
    public Entity entity;
    public bool isUser;
    public bool isContorller;
    public int id;
}