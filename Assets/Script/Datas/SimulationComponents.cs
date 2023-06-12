
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

public struct ControllerTag : IComponentData{} // 这个组件可能多个客户端不一致。
