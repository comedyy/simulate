
using Unity.Entities;
using Unity.Mathematics;

public struct LTransformComponet : IComponentData
{
    public float3 position;
    public quaternion rotation;
}

public struct LMoveByDirComponent : IComponentData
{
    public quaternion dir;
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
    public float aiInterval;
    public float despawnTime;
}

public struct SpawnMonsterComponent: IComponentData
{
    public int maxCount;
    public int currentCount;
    public float interval;
    public float spawnCountPerInterval;
    public float lastSpawnTime;
}


public struct MonsterAiComponent : IComponentData
{
    public float randomTurnInterval;
    public float lastTurnTime;
}

public struct MonsterAutoDespawnComponent : IComponentData
{
    public float despawnTime;
}

public struct LogicTime : IComponentData
{
    public int frameCount;
    public float escaped;
    public float deltaTime;
}

public struct VLerpTransformCopmnet : IComponentData
{
    public float3 preLogicPos;
    public quaternion preLogicRatation;
    public float lerpTime;
}


public struct VSpawnEvent : IComponentData
{
    public Entity target;
    public bool isUser;
}


public struct VDespawnEvent : IComponentData
{
    public Entity target;
}