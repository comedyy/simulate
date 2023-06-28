using Unity.Entities;

public struct HpComponent : IComponentData
{
    public int hp;
    public int hpMax;
}