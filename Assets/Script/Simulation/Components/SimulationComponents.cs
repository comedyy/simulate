
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct LRvoComponent : IComponentData
{
    public int rvoId;
}

public struct LTransformComponet : IComponentData
{
    public float3 position;
    public quaternion rotation;
}

public struct LMoveByDirComponent : IComponentData
{
    public float3 dir;
}

public struct LMoveByPosComponent : IComponentData
{
    public float3 pos;
}

public struct MoveSpeedComponent : IComponentData
{
    public float speed;
}

public struct SizeComponent : IComponentData
{
    public float size;
}

public struct SpawnEvent : IComponentData
{
    public float3 position;
    public float3 dir;
    public bool isUser;
    public float aiInterval;
    public float despawnTime;
    internal int id;
    public int hp;
    public int atk;
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
    public float3 lerpBeginPos;  // lerp开始的位置。
    public quaternion lerpBeginRotation; // lerp开始的rotation
    public float lerpTime;              // 已经lerp多久了。
}

public class GameObjectBindingComponent : IComponentData, IEquatable<GameObjectBindingComponent>
{
    public GameObject obj;
    public GameObject objFollow;

    public bool Equals(GameObjectBindingComponent other)
    {
        return other.obj == obj;
    }

    public override int GetHashCode()
    {
        return obj.GetHashCode();
    }
}


public struct ControllerHolder : IComponentData
{
    public Entity controller;
}

public struct UserListComponent : IComponentData
{
    public UnsafeList<Entity> allUser;
}

public struct SkillComponent : IComponentData
{
    public float interval;
    public float range;
    public float preHurtTime;
}

public struct UserComponnet : IComponentData
{
    public int id;
}

public struct UserAiComponent : IComponentData
{
    public float3 offsetToController;
}