using Unity.Entities;

public struct HurtComponent : IBufferElementData
{
    public Entity target;
    public int value;
}

public struct VHurtComponent : IBufferElementData
{
    public Entity target;
    public int value;
}