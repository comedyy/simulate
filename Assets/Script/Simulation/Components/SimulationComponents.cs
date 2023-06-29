
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
    public fp3 position;
    public fp3 rotation;
    public fp3 velocity;
}

public struct LMoveByDirComponent : IComponentData
{
    public fp3 dir;
}


public struct MoveSpeedComponent : IComponentData
{
    public fp speed;
}

public struct SizeComponent : IComponentData
{
    public fp size;
}

public struct SpawnEvent : IComponentData
{
    public fp3 position;
    public fp3 dir;
    public bool isUser;
    public fp aiInterval;
    public fp despawnTime;
    internal int id;
    public int hp;
    public int atk;
}

public struct SpawnMonsterComponent: IComponentData
{
    public int maxCount;
    public int currentCount;
    public fp interval;
    public fp spawnCountPerInterval;
    public fp lastSpawnTime;
}


public struct MonsterAiComponent : IComponentData
{
    public fp randomTurnInterval;
}

public struct MonsterAutoDespawnComponent : IComponentData
{
    public fp despawnTime;
}

public struct LogicTime : IComponentData
{
    public int frameCount;
    public fp escaped;
    public fp deltaTime;
}

public struct VLerpTransformCopmnet : IComponentData
{
    public float3 lerpBeginPos;  // lerp开始的位置。
    public quaternion lerpBeginRotation; // lerp开始的rotation
    public float lerpTime;              // 已经lerp多久了。
}

public struct UserMoveState : IComponentData
{
    public bool isMoving;
}

public class GameObjectBindingComponent : IComponentData
{
    public GameObject obj;
    public GameObject objFollow;
    public Animator animator;
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
    public fp interval;
    public fp range;
    public fp preHurtTime;
}

public struct UserComponnet : IComponentData
{
    public int id;
}

public struct UserAiComponent : IComponentData
{
    public fp3 offsetToController;
}