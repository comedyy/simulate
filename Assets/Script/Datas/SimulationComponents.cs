
using Unity.Entities;
using Unity.Mathematics;

public struct LTransformComponet : IComponentData
{
    public float3 position;
    public quaternion rotation;
}

public struct LMoveByDirComponent : IComponentData
{
    public float dir;
}

public struct LMoveByPosComponent : IComponentData
{
    public float3 pos;
}

public struct MoveSpeedComponent : IComponentData
{
    public float speed;
}

public struct SpawnEvent : IComponentData
{
    public float3 position;
    public quaternion dir;
    public bool isUser;
}

public struct SpawnMonsterComponent: IComponentData
{
    public int maxCount;
    public int currentCount;
    public float interval;
    public float lastSpawnTime;
}
